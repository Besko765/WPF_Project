using System.Windows;
using System.Windows.Controls;
using LanguageLearningApp.Data;
using LanguageLearningApp.Models;
using System.Linq;
using System;

namespace LanguageLearningApp.Views
{
    public partial class MultipleChoiceView : UserControl
    {
        private MainWindow mainWindow;
        private Set? currentSet;
        private Word? currentWord;
        private Random rnd = new Random();
        public MultipleChoiceView(MainWindow window)
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
            NextQuestion();
        }

        private void NextQuestion()
        {
            ResultMessage.Text = string.Empty;
            OptionsPanel.Children.Clear();

            if (currentSet == null || currentSet.Words.Count == 0)
            {
                QuestionText.Text = "Brak słów w zestawie.";
                return;
            }

            // wybierz losowe słowo jako pytanie (pokaż oryginał, odpowiedzi to tłumaczenia)
            currentWord = currentSet.Words[rnd.Next(currentSet.Words.Count)];
            QuestionText.Text = currentWord.Text;

            // przygotuj opcje (1 poprawna + do 3 losowe niepoprawne)
            var options = new System.Collections.Generic.List<string> { currentWord.Translation };
            var others = currentSet.Words.Where(w => w.Id != currentWord.Id).Select(w => w.Translation).Distinct().ToList();
            while (options.Count < Math.Min(4, 1 + others.Count))
            {
                if (others.Count == 0) break;
                var pick = others[rnd.Next(others.Count)];
                if (!options.Contains(pick)) options.Add(pick);
            }

            // shuffle
            options = options.OrderBy(_ => rnd.Next()).ToList();

            foreach (var opt in options)
            {
                var btn = new Button { Content = opt, Margin = new Thickness(4), Width = 320 }; 
                btn.Click += Option_Click;
                OptionsPanel.Children.Add(btn);
            }
        }

        private void Option_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button b && currentWord != null)
            {
                var sel = b.Content as string;
                if (string.Equals(sel, currentWord.Translation, StringComparison.InvariantCultureIgnoreCase))
                {
                    ResultMessage.Text = "Dobrze!";
                }
                else
                {
                    ResultMessage.Text = $"Źle. Poprawnie: {currentWord.Translation}";
                }
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            NextQuestion();
        }
    }
}