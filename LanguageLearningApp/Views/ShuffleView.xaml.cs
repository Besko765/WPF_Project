using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LanguageLearningApp.Data;
using LanguageLearningApp.Models;

namespace LanguageLearningApp.Views
{
	public partial class ShuffleView : UserControl
	{
		private MainWindow mainWindow;
		private Set? currentSet;
		private Word? currentWord;
		private string original = string.Empty;
		private string shuffled = string.Empty;

		public ShuffleView(MainWindow window)
		{
			InitializeComponent();
			mainWindow = window;

			SetSelector.ItemsSource = DataService.GetSets();
			SetSelector.SelectedIndex = 0;
		}

		private void Back_Click(object sender, RoutedEventArgs e)
		{
			mainWindow.Navigate(new GamesView(mainWindow));
		}

		private void NewGame_Click(object sender, RoutedEventArgs e)
		{
			StartNewGame();
		}

		private void StartNewGame()
		{
          ResultMessage.Text = string.Empty;
			LettersPanel.Children.Clear();
			AnswerBox.Text = string.Empty;

			currentSet = SetSelector.SelectedItem as Set;
			if (currentSet == null || currentSet.Words.Count == 0)
			{
				ResultMessage.Text = "Wybrany zestaw nie zawiera słów.";
				return;
			}

			var rnd = new System.Random();
           currentWord = currentSet.Words[rnd.Next(currentSet.Words.Count)];
			original = string.IsNullOrEmpty(currentWord.Translation) ? currentWord.Text : currentWord.Translation;
			var letters = original.Where(char.IsLetter).Select(char.ToLowerInvariant).ToArray();
			var shuffledArr = letters.OrderBy(_ => rnd.Next()).ToArray();
			shuffled = new string(shuffledArr);
			ShuffledWord.Text = string.Join(" ", shuffledArr);

			foreach (var ch in shuffledArr)
			{
				var btn = new Button { Content = ch.ToString(), Margin = new Thickness(4), MinWidth = 40 };
				btn.Click += Letter_Click;
				LettersPanel.Children.Add(btn);
			}
		}

		private static string NormalizeLetters(string s)
		{
			if (string.IsNullOrEmpty(s)) return string.Empty;
			return new string(s.Where(char.IsLetter).Select(char.ToLowerInvariant).ToArray());
		}

		private void Letter_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Button b && b.Content is string s)
			{
				AnswerBox.Text += s;
				b.IsEnabled = false;
			}
		}

		private void Check_Click(object sender, RoutedEventArgs e)
		{
          var user = NormalizeLetters(AnswerBox.Text);
			var target = NormalizeLetters(original);
			if (string.IsNullOrWhiteSpace(user)) return;
			if (user.Equals(target, System.StringComparison.InvariantCultureIgnoreCase))
			{
				ResultMessage.Text = "Dobrze!";
			}
			else
			{
				ResultMessage.Text = $"Źle. Poprawnie: {original}";
			}
		}

		private void ClearAnswer_Click(object sender, RoutedEventArgs e)
		{
			AnswerBox.Text = string.Empty;
			foreach (var child in LettersPanel.Children)
			{
				if (child is Button b) b.IsEnabled = true;
			}
			ResultMessage.Text = string.Empty;
		}
	}
}
