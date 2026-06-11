using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

namespace LanguageLearningApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public BackgroundMusicService? MusicService { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MusicService = new BackgroundMusicService();

            // domyślna ścieżka do pliku w projekcie: Assets/Audio/frutigga.mp3
            var relativePath = Path.Combine("Assets", "Audio", "frutigga.mp3");
            MusicService.PlayLoop(relativePath, 1);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            MusicService?.Stop();
            base.OnExit(e);
        }
    }

}
