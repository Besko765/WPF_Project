using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LanguageLearningApp.ViewModels;
using LanguageLearningApp.Views;
using LanguageLearningApp.Models;

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
			if (DataContext is SetsViewModel vm)
			{
				var temp = new Set { Id = (vm.Sets.Any() ? vm.Sets.Max(s => s.Id) : 0) + 1 };
				var win = new SetEditorWindow(temp, isEdit: false, readOnly: false) { Owner = Window.GetWindow(this) };
				if (win.ShowDialog() == true && win.EditedSet != null)
				{
					vm.Sets.Add(win.EditedSet);
				}
			}
		}

		private void OpenSet_Click(object sender, RoutedEventArgs e)
		{
			if (DataContext is SetsViewModel vm && vm.SelectedSet != null)
			{
				// open as read-only preview
				var win = new SetEditorWindow(vm.SelectedSet, isEdit: true, readOnly: true) { Owner = Window.GetWindow(this) };
				win.ShowDialog();
			}
		}

		private void EditSet_Click(object sender, RoutedEventArgs e)
		{
			if (DataContext is SetsViewModel vm && vm.SelectedSet != null)
			{
				var win = new SetEditorWindow(vm.SelectedSet, isEdit: true, readOnly: false) { Owner = Window.GetWindow(this) };
				if (win.ShowDialog() == true)
				{
					// changes already applied back to vm.SelectedSet inside dialog Save
				}
			}
		}

		private void SetsList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			// double-click opens editor for the selected set
			EditSet_Click(sender, e);
		}

		private void NameFilter_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (DataContext is SetsViewModel vm)
			{
				var filter = NameFilter.Text?.Trim();
				if (string.IsNullOrEmpty(filter))
				{
					SetsList.ItemsSource = vm.Sets;
				}
				else
				{
					SetsList.ItemsSource = vm.Sets.Where(s => s.Name.Contains(filter, System.StringComparison.InvariantCultureIgnoreCase)).ToList();
				}
			}
		}
	}
}