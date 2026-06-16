using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LanguageLearningApp.Data;
using LanguageLearningApp.Models;
using LanguageLearningApp.Services;

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
		private static AudioManager audio = new AudioManager();

		public HangmanView(MainWindow window)
			{
				InitializeComponent();
				mainWindow = window;

				SetSelector.ItemsSource = DataService.GetSets();
				SetSelector.SelectedIndex = 0;

				this.SizeChanged += HangmanView_SizeChanged;
				BuildKeyboard();
				// ustaw domyślne tło (pierwszy obraz z Resources/Images jeśli istnieje)
				TryLoadBackgroundImage();
				// inicjalizuj audio manager
				audio.Load();
				audio.PlayMusicLoop();

				// ⭐ Ustaw domyślny kolor przycisku wyciszenia na zielony (dźwięki włączone)
				if (MuteSfxButton != null)
				{
					MuteSfxButton.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(92, 184, 92)); // Green (#5CB85C)
				}
			}

		private void TryLoadBackgroundImage()
		{
			try
			{
				var folder = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Resources", "Images");
				if (!System.IO.Directory.Exists(folder)) return;
				var files = System.IO.Directory.GetFiles(folder)
					.Where(f => f.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase) || f.EndsWith(".jpg", System.StringComparison.OrdinalIgnoreCase) || f.EndsWith(".jpeg", System.StringComparison.OrdinalIgnoreCase))
					.ToArray();
				if (files.Length == 0) return;
				// wybierz losowo plik
				var rnd = new System.Random();
				var img = files[rnd.Next(files.Length)];
				// ustaw ImageSource przez dispatcher, bo kontrolka może nie być jeszcze zainicjalizowana
				System.Windows.Application.Current.Dispatcher.Invoke(() =>
				{
					BgImage.Opacity = 0; // zaczynamy od 0, zrobimy fade-in
					var uri = new System.Uri(img);
					var bmp = new System.Windows.Media.Imaging.BitmapImage();
					bmp.BeginInit();
					bmp.UriSource = uri;
					bmp.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
					bmp.EndInit();
					BgImage.Source = bmp;
					// zastosuj blur i wyblaknięcie przez efekt w kodzie-behind
					var blur = new System.Windows.Media.Effects.BlurEffect { Radius = 6 };
					BgImage.Effect = blur;
					var fade = new System.Windows.Media.Animation.DoubleAnimation(0, 0.7, new System.Windows.Duration(System.TimeSpan.FromMilliseconds(450)));
					BgImage.BeginAnimation(System.Windows.UIElement.OpacityProperty, fade);
				});
			}
			catch { }
		}

		private void ShowResult(bool win)
		{
			ResultOverlay.Visibility = Visibility.Visible;
			if (win)
			{
				ResultTitle.Text = "Wygrałeś! 🎉";
				ResultSubtitle.Text = "Świetnie! Chcesz spróbować jeszcze raz?";
				ResultOverlay.RenderTransform = new System.Windows.Media.ScaleTransform(0.9, 0.9);
				var grow = new System.Windows.Media.Animation.DoubleAnimation(0.9, 1, new System.Windows.Duration(System.TimeSpan.FromMilliseconds(300))) { EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut } };
				ResultOverlay.BeginAnimation(System.Windows.UIElement.OpacityProperty, new System.Windows.Media.Animation.DoubleAnimation(0,1, new System.Windows.Duration(System.TimeSpan.FromMilliseconds(300))));
				ResultOverlay.RenderTransform.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleXProperty, grow);
				ResultOverlay.RenderTransform.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleYProperty, grow);
			}
			else
			{
				ResultTitle.Text = "Przegrałeś";
				ResultSubtitle.Text = $"Poprawne słowo: {displayWord}";
			}

			foreach (var child in KeyboardGrid.Children)
			{
				if (child is Button b) b.IsEnabled = false;
			}
		}

		private void PlayAgainButton_Click(object sender, RoutedEventArgs e)
		{
			ResultOverlay.Visibility = Visibility.Collapsed;
			StartNewGame();
			// aktywuj klawiaturę
			foreach (var child in KeyboardGrid.Children)
			{
				if (child is Button b) b.IsEnabled = true;
			}
		}

		private void CloseOverlayButton_Click(object sender, RoutedEventArgs e)
		{
			ResultOverlay.Visibility = Visibility.Collapsed;
		}

		private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (audio == null) return;
			audio.MusicVolume = e.NewValue;
			System.Diagnostics.Debug.WriteLine($"Volume changed to: {e.NewValue}");
		}

		private void MuteSfx_Click(object sender, RoutedEventArgs e)
		{
			if (audio == null) return;
			audio.SfxMuted = !audio.SfxMuted;

			// Zmień tekst przycisku
			if (MuteSfxButton != null)
			{
				MuteSfxButton.Content = audio.SfxMuted ? "🔊 Włącz klawiaturę" : "🔇 Wycisz klawiaturę";

				// Zmień kolor przycisku: Red (wyciszone) lub Green (włączone)
				if (audio.SfxMuted)
				{
					MuteSfxButton.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(234, 84, 85)); // Red (#EA5455)
				}
				else
				{
					MuteSfxButton.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(92, 184, 92)); // Green (#5CB85C)
				}
			}

			System.Diagnostics.Debug.WriteLine($"SFX Muted: {audio.SfxMuted}");
		}

		private void BuildKeyboard()
		{
			string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			foreach (char ch in letters)
			{
				var btn = new Button { Content = ch.ToString(), Margin = new Thickness(4), Style = (Style)FindResource("KeyboardButton") };
				btn.Click += KeyboardLetter_Click;
				KeyboardGrid.Children.Add(btn);
			}
		}

		private void KeyboardLetter_Click(object? sender, RoutedEventArgs e)
		{
			if (sender is Button b && b.Content is string s && s.Length > 0)
			{
				LetterInput.Text = s;
				audio.PlayRandomSfx();
				Guess_Click(null, null);
				b.IsEnabled = false;
			}
		}

		private void LetterInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Enter)
			{
				audio.PlayRandomSfx();
				Guess_Click(null, null);
			}
			else if (e.Key == System.Windows.Input.Key.Escape)
			{
				StartNewGame();
			}
			else
			{
				// przy zwykłym naciśnięciu klawisza (litera) zagra dźwięk — ale tylko jeśli to litera
				if ((e.Key >= System.Windows.Input.Key.A && e.Key <= System.Windows.Input.Key.Z) || (e.Key >= System.Windows.Input.Key.NumPad0 && e.Key <= System.Windows.Input.Key.NumPad9) || (e.Key >= System.Windows.Input.Key.D0 && e.Key <= System.Windows.Input.Key.D9))
				{
					audio.PlayRandomSfx();
				}
			}
		}

		private void BuildMaskedWordVisuals()
		{
			MaskedPanel.Children.Clear();
			for (int i = 0; i < masked.Length; i++)
			{
				var tb = new TextBlock { Text = masked[i].ToString(), FontSize = 28, Margin = new Thickness(4), Opacity = 1 };
				MaskedPanel.Children.Add(tb);
			}
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

				// losuj slowo z zestawu
			  var rnd = new System.Random();
				currentWord = currentSet.Words[rnd.Next(currentSet.Words.Count)];
				displayWord = string.IsNullOrEmpty(currentWord.Translation) ? currentWord.Text : currentWord.Translation;
				masked = displayWord.Select(c => char.IsLetter(c) ? '_' : c).ToArray();
				BuildMaskedWordVisuals();
				// przy każdej nowej grze losuj nowe tło
				TryLoadBackgroundImage();
				LetterInput.Focus();

				// ⭐ Włącz wszystkie przyciski klawiatury
				foreach (var child in KeyboardGrid.Children)
				{
					if (child is Button b) b.IsEnabled = true;
				}

				UpdateUi();
			}


		private void UpdateUi()
		{
			for (int i = 0; i < masked.Length && i < MaskedPanel.Children.Count; i++)
			{
				if (MaskedPanel.Children[i] is TextBlock tb)
				{
					if (tb.Text != masked[i].ToString())
					{
						// animacja odslaniania litery
						tb.Text = masked[i].ToString();
						tb.RenderTransform = new System.Windows.Media.ScaleTransform(0.6, 0.6);
						var sb = new System.Windows.Media.Animation.Storyboard();
						var scaleX = new System.Windows.Media.Animation.DoubleAnimation(0.6, 1, new System.Windows.Duration(System.TimeSpan.FromMilliseconds(250))) { EasingFunction = new System.Windows.Media.Animation.BounceEase { Bounces = 1, Bounciness = 2 } };
						var scaleY = new System.Windows.Media.Animation.DoubleAnimation(0.6, 1, new System.Windows.Duration(System.TimeSpan.FromMilliseconds(250))) { EasingFunction = new System.Windows.Media.Animation.BounceEase { Bounces = 1, Bounciness = 2 } };
						System.Windows.Media.Animation.Storyboard.SetTarget(scaleX, tb);
						System.Windows.Media.Animation.Storyboard.SetTarget(scaleY, tb);
						System.Windows.Media.Animation.Storyboard.SetTargetProperty(scaleX, new PropertyPath("RenderTransform.ScaleX"));
						System.Windows.Media.Animation.Storyboard.SetTargetProperty(scaleY, new PropertyPath("RenderTransform.ScaleY"));
						sb.Children.Add(scaleX);
						sb.Children.Add(scaleY);
						sb.Begin();
					}
				}
			}

			GuessedLetters.Text = string.Join(", ", guessed);
			ErrorsCount.Text = errors.ToString();
			LetterInput.Text = string.Empty;
            if (currentWord == null) return;


			if (!masked.Contains('_'))
			{
				ShowResult(true);
			}
			else if (errors >= MaxErrors)
			{
				ShowResult(false);
			}

			DrawHangman(errors);
		}

		private void DrawHangman(int mistakes)
		{
			var c = HangmanCanvas;
			c.Children.Clear();

			double w = 220;
			double h = 260;

			double groundY = h - 20;
			double poleX = w * 0.18;
			double belkaEndX = w * 0.68;
			double headR = Math.Min(w, h) * 0.06;
			double headCenterX = belkaEndX;
			double headCenterY = h * 0.16; 

			double beamY = headCenterY - headR - 12;
			double ropeTopY = beamY + 2;
			double ropeBottomY = headCenterY - headR - 4;

			System.Windows.Media.Brush strokeBrush = System.Windows.Media.Brushes.Black;
			try { strokeBrush = (System.Windows.Media.Brush)FindResource("AccentBrush"); } catch { strokeBrush = System.Windows.Media.Brushes.Black; }

			c.Children.Add(new System.Windows.Shapes.Line { X1 = 10, Y1 = groundY, X2 = w - 10, Y2 = groundY, Stroke = strokeBrush, StrokeThickness = 4 });
			c.Children.Add(new System.Windows.Shapes.Line { X1 = poleX, Y1 = groundY, X2 = poleX, Y2 = beamY, Stroke = strokeBrush, StrokeThickness = 4 });
			c.Children.Add(new System.Windows.Shapes.Line { X1 = poleX, Y1 = beamY, X2 = belkaEndX, Y2 = beamY, Stroke = strokeBrush, StrokeThickness = 4 });
			c.Children.Add(new System.Windows.Shapes.Line { X1 = belkaEndX, Y1 = ropeTopY, X2 = belkaEndX, Y2 = ropeBottomY, Stroke = strokeBrush, StrokeThickness = 2 });

			if (mistakes >= 1)
			{
				// głowa
				var head = new System.Windows.Shapes.Ellipse { Width = headR * 2, Height = headR * 2, Stroke = strokeBrush, StrokeThickness = 2, Opacity = 0 };
				Canvas.SetLeft(head, headCenterX - headR);
				Canvas.SetTop(head, headCenterY - headR);
				c.Children.Add(head);
				var fade = new System.Windows.Media.Animation.DoubleAnimation(0, 1, new System.Windows.Duration(System.TimeSpan.FromMilliseconds(250)));
				head.BeginAnimation(System.Windows.UIElement.OpacityProperty, fade);
			}
			if (mistakes >= 2)
			{
				// tułów
				var body = new System.Windows.Shapes.Line { X1 = headCenterX, Y1 = headCenterY + headR * 0.6, X2 = headCenterX, Y2 = headCenterY + headR * 2.8, Stroke = strokeBrush, StrokeThickness = 2, Opacity = 0 };
				c.Children.Add(body);
				var fade2 = new System.Windows.Media.Animation.DoubleAnimation(0, 1, new System.Windows.Duration(System.TimeSpan.FromMilliseconds(250)));
				body.BeginAnimation(System.Windows.UIElement.OpacityProperty, fade2);
			}
			if (mistakes >= 3)
			{
				// ręka lewa
				var larm = new System.Windows.Shapes.Line { X1 = headCenterX, Y1 = headCenterY + headR * 0.9, X2 = headCenterX - headR * 1.6, Y2 = headCenterY + headR * 1.8, Stroke = strokeBrush, StrokeThickness = 2, Opacity = 0 };
				c.Children.Add(larm);
				var fade3 = new System.Windows.Media.Animation.DoubleAnimation(0, 1, new System.Windows.Duration(System.TimeSpan.FromMilliseconds(250)));
				larm.BeginAnimation(System.Windows.UIElement.OpacityProperty, fade3);
			}
			if (mistakes >= 4)
			{
				// ręka prawa
				var rarm = new System.Windows.Shapes.Line { X1 = headCenterX, Y1 = headCenterY + headR * 0.9, X2 = headCenterX + headR * 1.6, Y2 = headCenterY + headR * 1.8, Stroke = strokeBrush, StrokeThickness = 2, Opacity = 0 };
				c.Children.Add(rarm);
				var fade4 = new System.Windows.Media.Animation.DoubleAnimation(0, 1, new System.Windows.Duration(System.TimeSpan.FromMilliseconds(250)));
				rarm.BeginAnimation(System.Windows.UIElement.OpacityProperty, fade4);
			}
			if (mistakes >= 5)
			{
				// nogi
				var lleg = new System.Windows.Shapes.Line { X1 = headCenterX, Y1 = headCenterY + headR * 2.8, X2 = headCenterX - headR * 1.9, Y2 = headCenterY + headR * 4.6, Stroke = strokeBrush, StrokeThickness = 2, Opacity = 0 };
				var rleg = new System.Windows.Shapes.Line { X1 = headCenterX, Y1 = headCenterY + headR * 2.8, X2 = headCenterX + headR * 1.9, Y2 = headCenterY + headR * 4.6, Stroke = strokeBrush, StrokeThickness = 2, Opacity = 0 };
				c.Children.Add(lleg);
				c.Children.Add(rleg);
				var fade5 = new System.Windows.Media.Animation.DoubleAnimation(0, 1, new System.Windows.Duration(System.TimeSpan.FromMilliseconds(250)));
				lleg.BeginAnimation(System.Windows.UIElement.OpacityProperty, fade5);
				rleg.BeginAnimation(System.Windows.UIElement.OpacityProperty, fade5);
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
				var sb = new System.Windows.Media.Animation.Storyboard();
				var shake = new System.Windows.Media.Animation.DoubleAnimationUsingKeyFrames();
				shake.KeyFrames.Add(new System.Windows.Media.Animation.LinearDoubleKeyFrame(1, System.TimeSpan.FromMilliseconds(0)));
				shake.KeyFrames.Add(new System.Windows.Media.Animation.LinearDoubleKeyFrame(1.05, System.TimeSpan.FromMilliseconds(75)));
				shake.KeyFrames.Add(new System.Windows.Media.Animation.LinearDoubleKeyFrame(1, System.TimeSpan.FromMilliseconds(150)));
				System.Windows.Media.Animation.Storyboard.SetTarget(shake, GuessButton);
				System.Windows.Media.Animation.Storyboard.SetTargetProperty(shake, new PropertyPath("RenderTransform.ScaleX"));
				GuessButton.RenderTransform = new System.Windows.Media.ScaleTransform(1,1);
				sb.Children.Add(shake);
				sb.Begin();
				return;
			}

			guessed.Add(ch);
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
				var anim = new System.Windows.Media.Animation.DoubleAnimationUsingKeyFrames();
				anim.KeyFrames.Add(new System.Windows.Media.Animation.SplineDoubleKeyFrame(0, System.TimeSpan.FromMilliseconds(0)));
				anim.KeyFrames.Add(new System.Windows.Media.Animation.SplineDoubleKeyFrame(-6, System.TimeSpan.FromMilliseconds(60)));
				anim.KeyFrames.Add(new System.Windows.Media.Animation.SplineDoubleKeyFrame(6, System.TimeSpan.FromMilliseconds(120)));
				anim.KeyFrames.Add(new System.Windows.Media.Animation.SplineDoubleKeyFrame(0, System.TimeSpan.FromMilliseconds(180)));
				var transform = new System.Windows.Media.TranslateTransform();
				HangmanCanvas.RenderTransform = transform;
				transform.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, anim);
			}

			UpdateUi();
		}
	}
}
