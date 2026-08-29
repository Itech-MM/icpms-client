﻿using System.IO;
using System.Windows;
using log4net;
using Vlc.DotNet.Wpf;

namespace icpms_client.Tools.VideoPlayer
{
    public class RtspPlayer : IDisposable
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(RtspPlayer));
        private readonly VlcControl _vlcControl;
        private readonly DirectoryInfo _vlcLibDirectory;
        private bool _isRefreshing; // Prevents concurrent refresh operations

        public RtspPlayer(VlcControl vlcControl, string vlcPath = @"C:\Program Files\VideoLAN\VLC")
        {
            _vlcControl = vlcControl ?? throw new ArgumentNullException(nameof(vlcControl));
            _log.Debug($"VLC Path: {vlcPath}");
            _vlcLibDirectory = new DirectoryInfo(vlcPath);
            
            if (_vlcControl.SourceProvider.MediaPlayer == null)
            {
                _vlcControl.SourceProvider.CreatePlayer(_vlcLibDirectory);
            }
        }

        /// <summary>
        /// Starts playing the RTSP stream.
        /// </summary>
        public void Play(string rtspUrl, bool mute = true)
        {
            if (string.IsNullOrWhiteSpace(rtspUrl))
                throw new ArgumentException("RTSP URL cannot be empty", nameof(rtspUrl));

            Application.Current.Dispatcher.Invoke(() =>
            {
                _vlcControl.SourceProvider.MediaPlayer?.Play(new Uri(rtspUrl));
                if (_vlcControl.SourceProvider.MediaPlayer != null)
                    _vlcControl.SourceProvider.MediaPlayer.Audio.IsMute = mute;
            });
        }

        /// <summary>
        /// Refreshes the RTSP stream without freezing the UI.
        /// </summary>
        public async Task RefreshAsync(string rtspUrl, bool mute = true)
        {
            if (string.IsNullOrWhiteSpace(rtspUrl))
                throw new ArgumentException("RTSP URL cannot be empty", nameof(rtspUrl));

            if (_isRefreshing) return; // Prevent multiple refresh calls
            _isRefreshing = true;

            try
            {
                // Run stop & dispose operations in a background thread
                await Task.Run(async () =>
                {
                    _vlcControl.SourceProvider.MediaPlayer?.Stop();
                    await Task.Delay(300); // Allow stop to complete
                    _vlcControl.SourceProvider.MediaPlayer?.Dispose();
                    await Task.Delay(300); // Allow disposal to complete
                });

                // UI operations (reinitializing VLC player) must be done on UI thread
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _vlcControl.SourceProvider.CreatePlayer(_vlcLibDirectory);
                    _vlcControl.SourceProvider.MediaPlayer?.Play(new Uri(rtspUrl));
                    if (_vlcControl.SourceProvider.MediaPlayer != null)
                        _vlcControl.SourceProvider.MediaPlayer.Audio.IsMute = mute;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error refreshing RTSP stream: {ex.Message}");
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        /// <summary>
        /// Takes a snapshot of the current stream.
        /// </summary>
        public bool Snapshot(FileInfo file)
        {
            return _vlcControl.SourceProvider.MediaPlayer != null &&
                   _vlcControl.SourceProvider.MediaPlayer.TakeSnapshot(file);
        }

        /// <summary>
        /// Stops the RTSP stream.
        /// </summary>
        public void Stop()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _vlcControl.SourceProvider.MediaPlayer?.Stop();
            });
        }

        public async Task DisposeAsync()
        {
            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _vlcControl.SourceProvider.MediaPlayer?.Stop();
                    _vlcControl.SourceProvider.MediaPlayer?.Dispose();
                });
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
        }

        /// <summary>
        /// Cleans up resources.
        /// </summary>
        public void Dispose()
        {
            _vlcControl.SourceProvider.MediaPlayer?.Stop();
            _vlcControl.SourceProvider.MediaPlayer?.Dispose();
        }
    }
}