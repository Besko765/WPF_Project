using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LanguageLearningApp.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MainContent.Content = new MainMenuView(this);
        }

        public void Navigate(object view)
        {
            MainContent.Content = view;
        }

        private void MuteButton_Click(object sender, RoutedEventArgs e)
        {
            var app = System.Windows.Application.Current as App;
            if (app?.MusicService == null) return;

            if (app.MusicService.IsMuted)
            {
                app.MusicService.Unmute();
                MuteButton.Content = "Mute";
            }
            else
            {
                app.MusicService.Mute();
                MuteButton.Content = "Unmute";
            }
        }
    }
}