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

        private void Hangman_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.Navigate(new HangmanView(mainWindow));
        }

        private void Flashcards_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.Navigate(new FlashcardsView(mainWindow));
        }

        private void Quiz_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.Navigate(new MultipleChoiceView(mainWindow));
        }

        private void Match_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.Navigate(new MatchPairsView(mainWindow));
        }

        private void Shuffle_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.Navigate(new ShuffleView(mainWindow));
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.Navigate(new MainMenuView(mainWindow));
        }
    }
}