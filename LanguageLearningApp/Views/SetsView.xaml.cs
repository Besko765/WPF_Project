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

			var vm = new SetsViewModel();
			DataContext = vm;

			SetsList.ItemsSource = vm.Sets;
		}

		private void Back_Click(object sender, RoutedEventArgs e)
		{
			mainWindow.Navigate(new MainMenuView(mainWindow));
		}

		private void AddSet_Click(object sender, RoutedEventArgs e)
		{
			if (DataContext is SetsViewModel vm && vm.AddCommand.CanExecute(null))
			{
				vm.AddCommand.Execute(null);
			}
			else
			{
				System.Diagnostics.Debug.WriteLine("AddCommand nie może wykonać się (CanExecute == false) lub DataContext nie jest SetsViewModel.");
			}
		}

		private void OpenSet_Click(object sender, RoutedEventArgs e)
		{
			if (DataContext is SetsViewModel vm && vm.SelectedSet != null)
			{
				mainWindow.Navigate(new SetEditorView(mainWindow, vm.SelectedSet));
			}
		}
	}
}