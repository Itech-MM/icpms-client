using System;
using System.IO;
using System.Net.Security;
using System.Security.Authentication;
using System.Windows.Media.Imaging;
using FluentFTP;

namespace icpms_client.Utils.General
{
    public class FtpUtil(string host, string username, string password)
    {
        private FtpClient GetFtpClient()
        {
            var client = new FtpClient(host, username, password);
            client.Config.EncryptionMode = FtpEncryptionMode.Explicit;
            client.Config.SslProtocols = SslProtocols.Tls12;
            client.Config.SocketKeepAlive = false;

            client.ValidateCertificate += (control, e) => { e.Accept = true; };

            client.Connect();
            return client;
        }

        // Upload File
        public bool UploadFile(string localFilePath, string remoteFilePath)
        {
            try
            {
                using var client = GetFtpClient(); // Reuse connection setup method
                client.UploadFile(localFilePath, remoteFilePath, FtpRemoteExists.Overwrite, true);
                client.Disconnect();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FTP Upload Error: {ex.Message}");
                return false;
            }
        }

        // Download File
        public bool DownloadFile(string remoteFilePath, string localFilePath)
        {
            try
            {
                using var client = GetFtpClient(); // Reuse connection setup method
                client.DownloadFile(localFilePath, remoteFilePath);
                client.Disconnect();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FTP Download Error: {ex.Message}");
                return false;
            }
        }

        // Delete File
        public bool DeleteFile(string remoteFilePath)
        {
            try
            {
                using var client = GetFtpClient(); // Reuse connection setup method
                client.DeleteFile(remoteFilePath);
                client.Disconnect();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FTP Delete Error: {ex.Message}");
                return false;
            }
        }

        // List Directory Contents
        public string[] ListDirectory(string remotePath)
        {
            try
            {
                using var client = GetFtpClient();
                var items = client.GetListing(remotePath);
                client.Disconnect();
                return Array.ConvertAll(items, item => item.FullName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FTP List Error: {ex.Message}");
                return [];
            }
        }
        public BitmapImage? GetImageFromFtp(string remoteFilePath)
        {
            try
            {
                using var client = GetFtpClient();
                using var stream = new MemoryStream();
                
                if (client.DownloadStream(stream, remoteFilePath))
                {
                    stream.Position = 0;
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    client.Disconnect();
                    return bitmap;
                }
                
                Console.WriteLine("FTP Image Download Failed");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FTP Image Load Error: {ex.Message}");
                return null;
            }
        }
    }
}
