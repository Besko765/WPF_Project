using System.Windows;
using System.Windows.Controls;

namespace LanguageLearningApp.Views
{
    public partial class GamesView : UserControl
    {
        private MainWindow mainWindow;

        public GamesView(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.Navigate(new MainMenuView(mainWindow));
        }
    }
}