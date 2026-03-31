using System.Windows;
using System.Windows.Controls;
using LanguageLearningApp.ViewModels;

namespace LanguageLearningApp.Views
{
	public partial class SetsView : UserControl
	{
		private MainWindow mainWindow;

		public SetsView(MainWindow window)
		{
			InitializeComponent();
			mainWindow = window;

			DataContext = new SetsViewModel();

			if (DataContext is SetsViewModel vm)
			{
				SetsList.ItemsSource = vm.Sets;
			}
		}

		private void Back_Click(object sender, RoutedEventArgs e)
		{
			mainWindow.Navigate(new MainMenuView(mainWindow));
		}

		// Fallback: wywołaj komendę Add bezpośrednio z code-behind
		private void AddSet_Click(object sender, RoutedEventArgs e)
		{
			if (DataContext is SetsViewModel vm && vm.AddCommand.CanExecute(null))
			{
				vm.AddCommand.Execute(null);
			}
			else
			{
				// pomocnicze logowanie do Output (opcjonalne)
				System.Diagnostics.Debug.WriteLine("AddCommand nie może wykonać się (CanExecute == false) lub DataContext nie jest SetsViewModel.");
			}
		}
	}
}