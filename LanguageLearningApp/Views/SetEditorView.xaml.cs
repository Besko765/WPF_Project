using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LanguageLearningApp.Models;
using LanguageLearningApp.Data;

namespace LanguageLearningApp.Views
{
	public partial class SetEditorView : UserControl
	{
		private MainWindow mainWindow;
		private Set editingSet;

		public SetEditorView(MainWindow window, Set set)
		{
			InitializeComponent();
			mainWindow = window;
			editingSet = set;

			SetName.Text = editingSet.Name;
			WordsList.ItemsSource = editingSet.Words;
		}

		private void Back_Click(object sender, RoutedEventArgs e)
		{
			mainWindow.Navigate(new SetsView(mainWindow));
		}

		private void Add_Click(object sender, RoutedEventArgs e)
		{
			var text = WordText.Text?.Trim();
			var trans = WordTranslation.Text?.Trim();
			if (string.IsNullOrEmpty(text)) return;
			var newId = (editingSet.Words.Any() ? editingSet.Words.Max(w => w.Id) : 0) + 1;
			editingSet.Words.Add(new Word { Id = newId, SetId = editingSet.Id, Text = text, Translation = trans ?? string.Empty });
			WordText.Text = string.Empty;
			WordTranslation.Text = string.Empty;
		}

		private void Update_Click(object sender, RoutedEventArgs e)
		{
			if (WordsList.SelectedItem is Word w)
			{
				int idx = editingSet.Words.IndexOf(w);
				if (idx < 0) return;
				editingSet.Words[idx] = new Word { Id = w.Id, SetId = editingSet.Id, Text = WordText.Text ?? string.Empty, Translation = WordTranslation.Text ?? string.Empty };
				WordsList.SelectedItem = editingSet.Words[idx];
			}
		}

		private void Delete_Click(object sender, RoutedEventArgs e)
		{
			if (WordsList.SelectedItem is Word w)
			{
				editingSet.Words.Remove(w);
			}
		}
	}
}
