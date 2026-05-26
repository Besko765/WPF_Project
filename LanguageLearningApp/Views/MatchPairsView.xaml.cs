using System.Windows;
using System.Windows.Controls;
using LanguageLearningApp.Data;
using LanguageLearningApp.Models;
using System.Linq;
using System;

namespace LanguageLearningApp.Views
{
    public partial class MatchPairsView : UserControl
    {
        private MainWindow mainWindow;
        private Set? currentSet;
        private Random rnd = new Random();
        private string? leftSelected = null;
        private System.Collections.Generic.Dictionary<string, string> pairs = new System.Collections.Generic.Dictionary<string, string>();
        public MatchPairsView(MainWindow window)
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
            BuildPairs();
        }

        private void BuildPairs()
        {
            LeftPanel.Children.Clear();
            RightPanel.Children.Clear();
            ResultMessage.Text = string.Empty;
            leftSelected = null;
            pairs.Clear();

            if (currentSet == null || currentSet.Words.Count == 0)
            {
                LeftPanel.Children.Add(new TextBlock { Text = "Brak słów." });
                return;
            }

            var items = currentSet.Words.ToList();
            var lefts = items.Select(w => w.Text).OrderBy(_ => rnd.Next()).ToList();
            var rights = items.Select(w => w.Translation).OrderBy(_ => rnd.Next()).ToList();

            // store correct mapping
            foreach (var w in items) pairs[w.Text] = w.Translation;

            foreach (var l in lefts)
            {
                var btn = new Button { Content = l, Margin = new Thickness(4) };
                btn.Click += Left_Click;
                LeftPanel.Children.Add(btn);
            }

            foreach (var r in rights)
            {
                var btn = new Button { Content = r, Margin = new Thickness(4) };
                btn.Click += Right_Click;
                RightPanel.Children.Add(btn);
            }
        }

        private void Left_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button b)
            {
                leftSelected = b.Content as string;
                ResultMessage.Text = $"Wybrano: {leftSelected}. Teraz kliknij odpowiadającą prawą.";
            }
        }

        private void Right_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button b && leftSelected != null)
            {
                var right = b.Content as string;
                if (pairs.TryGetValue(leftSelected, out var correct) && string.Equals(correct, right, StringComparison.InvariantCultureIgnoreCase))
                {
                    ResultMessage.Text = "Dobrze!";
                    // disable matched buttons
                    DisableButtonByContent(LeftPanel, leftSelected);
                    DisableButtonByContent(RightPanel, right);
                }
                else
                {
                    ResultMessage.Text = $"Źle. {leftSelected} -> {correct}";
                }

                leftSelected = null;
            }
        }

        private void DisableButtonByContent(Panel panel, string? content)
        {
            if (content == null) return;
            foreach (var child in panel.Children)
            {
                if (child is Button b && (b.Content as string) == content)
                {
                    b.IsEnabled = false;
                    break;
                }
            }
        }
    }
}