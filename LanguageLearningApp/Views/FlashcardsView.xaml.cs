using System.Windows;
using System.Windows.Controls;
using LanguageLearningApp.Data;
using LanguageLearningApp.Models;
using System.Linq;

namespace LanguageLearningApp.Views
{
    public partial class FlashcardsView : UserControl
    {
        private MainWindow mainWindow;
        private Set? currentSet;
        private Word? currentWord;
        private int currentIndex = -1;
        public FlashcardsView(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window;

            SetSelector.ItemsSource = DataService.GetSets();
            if (SetSelector.Items.Count > 0) SetSelector.SelectedIndex = 0;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.Navigate(new GamesView(mainWindow));
        }

        private void SetSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentSet = SetSelector.SelectedItem as Set;
            currentIndex = -1;
            ShowNextCard();
        }

        private void ShowNextCard()
        {
            CardBack.Visibility = Visibility.Collapsed;
            ResultMessage.Text = string.Empty;
            if (currentSet == null || currentSet.Words.Count == 0)
            {
                CardFront.Text = "Brak słów w wybranym zestawie.";
                CardBack.Text = string.Empty;
                return;
            }

            currentIndex = (currentIndex + 1) % currentSet.Words.Count;
            currentWord = currentSet.Words[currentIndex];
            // front = original text, back = translation (learn new language)
            CardFront.Text = currentWord.Text;
            CardBack.Text = currentWord.Translation;
        }

        private void ShowAnswer_Click(object sender, RoutedEventArgs e)
        {
            if (currentWord == null) return;
            CardBack.Visibility = Visibility.Visible;
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            ShowNextCard();
        }
    }
}