using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using LanguageLearningApp.Models;
using LanguageLearningApp.Data;

namespace LanguageLearningApp.Views
{
    public partial class SetsView : UserControl
    {
        private MainWindow mainWindow;
        private List<Set> sets;

        public SetsView(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window;

            sets = DataService.GetSets();
            SetsList.ItemsSource = sets;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.Navigate(new MainMenuView(mainWindow));
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

            SetsList.ItemsSource = null;
            SetsList.ItemsSource = sets;

            NameInput.Text = "";
            OgLangInput.Text = "";
            NewLangInput.Text = "";
        }
    }
}