using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LanguageLearningApp.Data;
using LanguageLearningApp.Models;

namespace LanguageLearningApp.Views
{
	public partial class HangmanView : UserControl
	{
		private MainWindow mainWindow;
		private Set? currentSet;
		private Word? currentWord;
      private string displayWord = string.Empty;
		private char[] masked;
		private System.Collections.Generic.HashSet<char> guessed = new System.Collections.Generic.HashSet<char>();
		private int errors = 0;
        private const int MaxErrors = 5;
		private const int VisualMaxErrors = 5; // ile elementów będzie rysowanych

		public HangmanView(MainWindow window)
		{
			InitializeComponent();
			mainWindow = window;

			SetSelector.ItemsSource = DataService.GetSets();
			SetSelector.SelectedIndex = 0;

			this.SizeChanged += HangmanView_SizeChanged;
		}

		private void HangmanView_SizeChanged(object? sender, SizeChangedEventArgs e)
		{
			if (this.ActualWidth <= 720)
			{
				RootWrap.Orientation = Orientation.Vertical;
				foreach (var child in RootWrap.Children)
				{
					if (child is FrameworkElement fe) fe.Width = double.NaN;
				}
			}
			else
			{
				RootWrap.Orientation = Orientation.Horizontal;
				foreach (var child in RootWrap.Children)
				{
					if (child is FrameworkElement fe) fe.Width = double.NaN;
				}
			}
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
			Message.Text = string.Empty;
			guessed.Clear();
			errors = 0;
			ErrorsCount.Text = errors.ToString();

			currentSet = SetSelector.SelectedItem as Set;
			if (currentSet == null || currentSet.Words.Count == 0)
			{
				Message.Text = "Wybrany zestaw nie zawiera słów.";
				return;
			}

			// losuj słowo z zestawu
          var rnd = new System.Random();
			currentWord = currentSet.Words[rnd.Next(currentSet.Words.Count)];
			// używamy tłumaczenia jako słowa do odgadnięcia (język obcy)
			displayWord = string.IsNullOrEmpty(currentWord.Translation) ? currentWord.Text : currentWord.Translation;
			masked = displayWord.Select(c => char.IsLetter(c) ? '_' : c).ToArray();
			UpdateUi();
		}

       // gdy używamy Viewbox z ustaloną szerokością/wyświetlaniem, nie trzeba reagować na SizeChanged

		private void UpdateUi()
		{
			MaskedWord.Text = string.Join(" ", masked);
			GuessedLetters.Text = string.Join(", ", guessed);
			ErrorsCount.Text = errors.ToString();
			LetterInput.Text = string.Empty;
            if (currentWord == null) return;


            if (!masked.Contains('_'))
			{
				Message.Text = "Wygrałeś!";
			}
			else if (errors >= MaxErrors)
			{
             Message.Text = $"Przegrałeś. Poprawne słowo: {displayWord}";
			}

			// narysuj wizualizację wisielca
			DrawHangman(errors);
		}

		private void DrawHangman(int mistakes)
		{
          // rysuj proporcjonalnie do rozmiaru canvasa
			var c = HangmanCanvas;
			c.Children.Clear();

         // używamy stałego układu współrzędnych 220x260 (Canvas) - dzięki Viewbox będzie skalowane
			double w = 220;
			double h = 260;

			double groundY = h - 20;
			double poleX = w * 0.18;
			double belkaEndX = w * 0.68;
			double ropeX = belkaEndX;
			double headCenterX = ropeX;
			double headTopY = h * 0.12;

			// podstawowa konstrukcja szubienicy
			c.Children.Add(new System.Windows.Shapes.Line { X1 = 10, Y1 = groundY, X2 = w - 10, Y2 = groundY, Stroke = System.Windows.Media.Brushes.Black, StrokeThickness = 4 });
			c.Children.Add(new System.Windows.Shapes.Line { X1 = poleX, Y1 = groundY, X2 = poleX, Y2 = headTopY - 10, Stroke = System.Windows.Media.Brushes.Black, StrokeThickness = 4 });
			c.Children.Add(new System.Windows.Shapes.Line { X1 = poleX, Y1 = headTopY - 10, X2 = belkaEndX, Y2 = headTopY - 10, Stroke = System.Windows.Media.Brushes.Black, StrokeThickness = 4 });
			c.Children.Add(new System.Windows.Shapes.Line { X1 = ropeX, Y1 = headTopY - 10, X2 = ropeX, Y2 = headTopY + 20, Stroke = System.Windows.Media.Brushes.Black, StrokeThickness = 2 });

			// rysuj elementy w zależności od liczby błędów (maksymalnie VisualMaxErrors)
			if (mistakes >= 1)
			{
				// głowa
				double headR = Math.Min(w, h) * 0.06;
				var head = new System.Windows.Shapes.Ellipse { Width = headR * 2, Height = headR * 2, Stroke = System.Windows.Media.Brushes.Black, StrokeThickness = 2 };
				Canvas.SetLeft(head, headCenterX - headR);
				Canvas.SetTop(head, headTopY);
				c.Children.Add(head);
			}
			if (mistakes >= 2)
			{
				// tułów
				var body = new System.Windows.Shapes.Line { X1 = ropeX, Y1 = headTopY + Math.Min(w, h) * 0.12, X2 = ropeX, Y2 = headTopY + Math.Min(w, h) * 0.35, Stroke = System.Windows.Media.Brushes.Black, StrokeThickness = 2 };
				c.Children.Add(body);
			}
			if (mistakes >= 3)
			{
				// ręka lewa
				var larm = new System.Windows.Shapes.Line { X1 = ropeX, Y1 = headTopY + Math.Min(w, h) * 0.18, X2 = ropeX - Math.Min(w, h) * 0.12, Y2 = headTopY + Math.Min(w, h) * 0.32, Stroke = System.Windows.Media.Brushes.Black, StrokeThickness = 2 };
				c.Children.Add(larm);
			}
			if (mistakes >= 4)
			{
				// ręka prawa
				var rarm = new System.Windows.Shapes.Line { X1 = ropeX, Y1 = headTopY + Math.Min(w, h) * 0.18, X2 = ropeX + Math.Min(w, h) * 0.12, Y2 = headTopY + Math.Min(w, h) * 0.32, Stroke = System.Windows.Media.Brushes.Black, StrokeThickness = 2 };
				c.Children.Add(rarm);
			}
			if (mistakes >= 5)
			{
				// nogi
				var lleg = new System.Windows.Shapes.Line { X1 = ropeX, Y1 = headTopY + Math.Min(w, h) * 0.35, X2 = ropeX - Math.Min(w, h) * 0.16, Y2 = headTopY + Math.Min(w, h) * 0.6, Stroke = System.Windows.Media.Brushes.Black, StrokeThickness = 2 };
				var rleg = new System.Windows.Shapes.Line { X1 = ropeX, Y1 = headTopY + Math.Min(w, h) * 0.35, X2 = ropeX + Math.Min(w, h) * 0.16, Y2 = headTopY + Math.Min(w, h) * 0.6, Stroke = System.Windows.Media.Brushes.Black, StrokeThickness = 2 };
				c.Children.Add(lleg);
				c.Children.Add(rleg);
			}
		}

		private void Guess_Click(object sender, RoutedEventArgs e)
		{
			if (currentWord == null) return;
			var input = LetterInput.Text?.Trim();
			if (string.IsNullOrEmpty(input)) return;
			char ch = input[0];
			ch = char.ToLowerInvariant(ch);

			if (guessed.Contains(ch))
			{
				Message.Text = "Już zgadywałeś tę literę.";
				return;
			}

			guessed.Add(ch);
           // sprawdzamy w displayWord (język obcy / Translation)
			if (displayWord.ToLowerInvariant().Contains(ch))
			{
				for (int i = 0; i < displayWord.Length; i++)
				{
					if (char.ToLowerInvariant(displayWord[i]) == ch)
						masked[i] = displayWord[i];
				}
			}
			else
			{
				errors++;
			}

			UpdateUi();
		}
	}
}
