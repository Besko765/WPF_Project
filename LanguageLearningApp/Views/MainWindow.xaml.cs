using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LanguageLearningApp.Data;
using System.Collections.Generic;
using LanguageLearningApp.Models;

namespace LanguageLearningApp.Views
{
	public partial class MainWindow : Window
	{
		private List<Set> sets;

		public MainWindow()
		{
			InitializeComponent();

			sets = DataService.GetSets();
			SetsList.ItemsSource = sets;
		}

		private void AddSet_Click(object sender, RoutedEventArgs e)
		{
			var newSet = new Set
			{
				Id = sets.Count + 1,
				Name = NameInput.Text,
				OgLanguage = OgLangInput.Text,
				NewLanguage = NewLangInput.Text
			};

			sets.Add(newSet);

			// odświeżenie listy
			SetsList.ItemsSource = null;
			SetsList.ItemsSource = sets;

			// wyczyszczenie formularza
			NameInput.Text = "";
			OgLangInput.Text = "";
			NewLangInput.Text = "";
		}
	}
}