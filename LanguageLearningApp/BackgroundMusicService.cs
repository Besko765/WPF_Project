using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;

namespace LanguageLearningApp
{
    public class BackgroundMusicService
    {
        private readonly MediaPlayer _player = new();
        private double _lastVolume = 0.3;
        private bool _isMuted = false;

        public void PlayLoop(string relativePathOrAbsolute, double volume = 0.3)
        {
            string path = GetPath(relativePathOrAbsolute);

            _player.Volume = Math.Clamp(volume, 0.0, 1.0);
            _player.MediaEnded += Player_MediaEnded;
            _player.MediaOpened += Player_MediaOpened;
            _player.MediaFailed += Player_MediaFailed;

            if (File.Exists(path))
            {
                Debug.WriteLine($"BackgroundMusicService: Playing file from disk: {path}");
                _player.Open(new Uri(path, UriKind.Absolute));
                _player.Play();
                return;
            }

            // Try site-of-origin pack URI (for Content files copied to output)
            var siteOfOrigin = new Uri($"pack://siteoforigin:,,,/{relativePathOrAbsolute.Replace('\\', '/')}");
            try
            {
                Debug.WriteLine($"BackgroundMusicService: Trying siteoforigin URI: {siteOfOrigin}");
                _player.Open(siteOfOrigin);
                _player.Play();
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BackgroundMusicService: siteoforigin open failed: {ex.Message}");
            }

            // Try application resource pack URI (if file was added as Resource)
            var assemblyName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "";
            var appPack = new Uri($"pack://application:,,,/{assemblyName};component/{relativePathOrAbsolute.Replace('\\', '/')}" , UriKind.Absolute);
            try
            {
                Debug.WriteLine($"BackgroundMusicService: Trying application pack URI: {appPack}");
                _player.Open(appPack);
                _player.Play();
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BackgroundMusicService: application pack open failed: {ex.Message}");
            }

            Debug.WriteLine($"BackgroundMusicService: Audio file not found: {relativePathOrAbsolute}. Ensure Build Action=Content and Copy to Output Directory=Copy if newer.");
        }

        private void Player_MediaEnded(object? sender, EventArgs e)
        {
            _player.Position = TimeSpan.Zero;
            _player.Play();
        }

        private void Player_MediaOpened(object? sender, EventArgs e)
        {
            Debug.WriteLine("BackgroundMusicService: Media opened successfully.");
        }

        private void Player_MediaFailed(object? sender, ExceptionEventArgs e)
        {
            Debug.WriteLine($"BackgroundMusicService: Media failed: {e.ErrorException.Message}");
        }

        public void Stop()
        {
            _player.Stop();
            _player.Close();
            _player.MediaEnded -= Player_MediaEnded;
        }

        public void SetVolume(double volume)
        {
            var v = Math.Clamp(volume, 0.0, 1.0);
            _player.Volume = v;
            if (!_isMuted) _lastVolume = v;
        }

        public double CurrentVolume => _player.Volume;

        public void Mute()
        {
            if (_isMuted) return;
            _lastVolume = _player.Volume;
            _player.Volume = 0.0;
            _isMuted = true;
        }

        public void Unmute()
        {
            if (!_isMuted) return;
            _player.Volume = Math.Clamp(_lastVolume, 0.0, 1.0);
            _isMuted = false;
        }

        public bool IsMuted => _isMuted;

        private static string GetPath(string path)
        {
            // If absolute path provided, return as-is
            if (Path.IsPathRooted(path)) return path;

            // Otherwise assume path is relative to AppDomain.CurrentDomain.BaseDirectory
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
        }
    }
}
