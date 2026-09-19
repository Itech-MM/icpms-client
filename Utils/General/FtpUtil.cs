using System;
using System.IO;
using System.Net.Security;
using System.Security.Authentication;
using System.Threading;
using System.Windows.Media.Imaging;
using FluentFTP;
using FluentFTP.Helpers;

namespace icpms_client.Utils.General
{
    public class FtpUtil : IDisposable
    {
        private readonly string _host;
        private readonly string _username;
        private readonly string _password;
        private readonly SemaphoreSlim _lock = new(1, 1);
        private FtpClient? _client;

        // Cache up to 100 images (tune to taste — each is decoded pixel data, so
        // keep it reasonable relative to typical photo dimensions).
        private readonly FtpImageCache _imageCache = new(maxItems: 100);

        public FtpUtil(string host, string username, string password)
        {
            _host = host;
            _username = username;
            _password = password;
        }

        private FtpClient GetConnectedClient()
        {
            if (_client is { IsConnected: true })
                return _client;

            _client?.Dispose();

            var client = new FtpClient(_host, _username, _password);
            client.Config.EncryptionMode = FtpEncryptionMode.Explicit;
            client.Config.SslProtocols = SslProtocols.Tls12;
            client.Config.SocketKeepAlive = false;
            client.ValidateCertificate += (control, e) => { e.Accept = true; };

            client.Connect();
            _client = client;
            return _client;
        }

        private T RunLocked<T>(Func<FtpClient, T> action, T failureValue, string opName, string context)
        {
            _lock.Wait();
            try
            {
                try
                {
                    var client = GetConnectedClient();
                    return action(client);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"FTP {opName} Error ({context}): {ex.Message}. Retrying with fresh connection...");
                    _client?.Dispose();
                    _client = null;

                    try
                    {
                        var client = GetConnectedClient();
                        return action(client);
                    }
                    catch (Exception ex2)
                    {
                        Console.WriteLine($"FTP {opName} Error ({context}) after retry: {ex2.Message}");
                        return failureValue;
                    }
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        public bool UploadFile(string localFilePath, string remoteFilePath) =>
            RunLocked(client =>
            {
                client.UploadFile(localFilePath, remoteFilePath, FtpRemoteExists.Overwrite, true);
                return true;
            }, false, "Upload", remoteFilePath);

        public bool UploadFile(byte[] fileBytes, string remoteFilePath) =>
            RunLocked(client =>
            {
                using var stream = new MemoryStream(fileBytes);
                client.UploadStream(stream, remoteFilePath, FtpRemoteExists.Overwrite, true);
                return true;
            }, false, "Upload", remoteFilePath);

        public bool UploadFile(byte[] fileBytes, string remoteFilePath, Action<FtpProgress> progress)
        {
            if (progress == null) throw new ArgumentNullException(nameof(progress));

            return RunLocked(client =>
            {
                using var stream = new MemoryStream(fileBytes);
                client.UploadStream(stream, remoteFilePath, FtpRemoteExists.Overwrite, true, progress);
                return true;
            }, false, "Upload", remoteFilePath);
        }

        public bool DownloadFile(string remoteFilePath, string localFilePath) =>
            RunLocked(client =>
                client.DownloadFile(localFilePath, remoteFilePath).IsSuccess(),
                false, "Download", remoteFilePath);

        public bool DeleteFile(string remoteFilePath) =>
            RunLocked(client =>
            {
                client.DeleteFile(remoteFilePath);
                return true;
            }, false, "Delete", remoteFilePath);

        public string[] ListDirectory(string remotePath) =>
            RunLocked(client =>
            {
                var items = client.GetListing(remotePath);
                return Array.ConvertAll(items, item => item.FullName);
            }, [], "List", remotePath);

        /// <summary>
        /// Loads an image from FTP, using an in-memory cache keyed by remote path.
        /// Cache is checked WITHOUT taking the FTP lock, so repeat views of the
        /// same photo return instantly without any network/FTP round trip.
        /// </summary>
        public BitmapImage? GetImageFromFtp(string remoteFilePath)
        {
            if (_imageCache.TryGet(remoteFilePath, out var cached))
            {
                return cached;
            }

            var image = RunLocked(client =>
            {
                using var stream = new MemoryStream();

                if (!client.DownloadStream(stream, remoteFilePath))
                {
                    Console.WriteLine($"FTP Image Download Failed: {remoteFilePath}");
                    return null;
                }

                stream.Position = 0;
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze(); // required so it's safe to cache + share across threads/bindings
                return bitmap;
            }, null, "ImageLoad", remoteFilePath);

            if (image != null)
            {
                _imageCache.Set(remoteFilePath, image);
            }

            return image;
        }

        /// <summary>Clears cached images — call if you need to force-refresh (e.g. photos were re-uploaded under the same path).</summary>
        public void ClearImageCache() => _imageCache.Clear();

        public void Dispose()
        {
            _lock.Wait();
            try
            {
                _client?.Dispose();
                _client = null;
            }
            finally
            {
                _lock.Release();
                _lock.Dispose();
            }
        }
    }
}