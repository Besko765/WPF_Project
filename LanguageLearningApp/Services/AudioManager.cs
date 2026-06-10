using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;

namespace LanguageLearningApp.Services
{
    public class AudioManager
    {
        private readonly string audioRoot;
        private List<Uri> sfxUris = new List<Uri>();
        private List<System.Media.SoundPlayer> soundPlayers = new List<System.Media.SoundPlayer>();
        private List<MediaPlayer> activeMediaPlayers = new List<MediaPlayer>();
        private Uri? musicUri;
        private MediaPlayer? musicPlayer;
        private readonly Random rnd = new Random();

        public double MusicVolume { get; set; } = 0.15; // cicho domyślnie
        public double SfxVolume { get; set; } = 0.8;

        public AudioManager(string? audioRootPath = null)
        {
            audioRoot = audioRootPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Audio");
        }

        public void Load()
        {
            if (!Directory.Exists(audioRoot)) return;

            var allFiles = Directory.GetFiles(audioRoot, "*.*", SearchOption.AllDirectories)
                .Where(f => f.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            // prefer files in a 'music' folder for music
            var musicFiles = allFiles.Where(p => p.Split(Path.DirectorySeparatorChar).Any(seg => seg.Equals("music", StringComparison.OrdinalIgnoreCase))).ToArray();
            if (musicFiles.Length > 0)
            {
                musicUri = new Uri(musicFiles[0]);
            }
            else
            {
                var fallback = allFiles.FirstOrDefault(f => f.IndexOf("music", StringComparison.OrdinalIgnoreCase) >= 0);
                if (fallback != null) musicUri = new Uri(fallback);
            }

            // sfx: prefer 'game' or 'sfx' folder, otherwise all non-music files
            var sfxFiles = allFiles.Where(p => p.Split(Path.DirectorySeparatorChar).Any(seg => seg.Equals("game", StringComparison.OrdinalIgnoreCase) || seg.Equals("sfx", StringComparison.OrdinalIgnoreCase))).ToArray();
            if (sfxFiles.Length == 0)
            {
                sfxFiles = allFiles.Where(f => musicUri == null || !string.Equals(new Uri(f).LocalPath, musicUri.LocalPath, StringComparison.OrdinalIgnoreCase)).ToArray();
            }

            sfxUris = sfxFiles.Select(f => new Uri(f)).ToList();

            // preload WAV SFX into SoundPlayer to avoid GC/latency issues
            soundPlayers.Clear();
            foreach (var uri in sfxUris)
            {
                try
                {
                    if (uri.LocalPath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
                    {
                        var sp = new System.Media.SoundPlayer(uri.LocalPath);
                        try { sp.LoadAsync(); } catch { }
                        soundPlayers.Add(sp);
                    }
                }
                catch { }
            }

            System.Diagnostics.Debug.WriteLine($"AudioManager: loaded music={(musicUri!=null)} sfx={sfxUris.Count} wavPlayers={soundPlayers.Count}");
        }

        public void PlayMusicLoop()
        {
            if (musicUri == null) return;
            if (musicPlayer != null)
            {
                musicPlayer.Stop();
                musicPlayer.Close();
            }

            musicPlayer = new MediaPlayer();
            musicPlayer.Open(musicUri);
            musicPlayer.Volume = MusicVolume;
            musicPlayer.MediaEnded += (s, e) =>
            {
                if (musicPlayer != null)
                {
                    musicPlayer.Position = TimeSpan.Zero;
                    musicPlayer.Play();
                }
            };
            musicPlayer.Play();
        }

        public void StopMusic()
        {
            if (musicPlayer == null) return;
            musicPlayer.Stop();
            musicPlayer.Close();
            musicPlayer = null;
        }

        public void PlayRandomSfx()
        {
            if (sfxUris == null || sfxUris.Count == 0) return;

            // prefer SoundPlayer (preloaded wav) for low-latency playback
            if (soundPlayers.Count > 0)
            {
                var idx = rnd.Next(soundPlayers.Count);
                try
                {
                    soundPlayers[idx].Play();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("AudioManager.PlayRandomSfx SoundPlayer failed: " + ex.Message);
                }
                return;
            }

            var uri = sfxUris[rnd.Next(sfxUris.Count)];
            var player = new MediaPlayer();
            activeMediaPlayers.Add(player);
            player.Open(uri);
            player.Volume = SfxVolume;
            player.MediaEnded += (s, e) =>
            {
                try { player.Close(); } catch { }
                activeMediaPlayers.Remove(player);
            };
            player.Play();
        }
    }
}
