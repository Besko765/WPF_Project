using System;
using System.Collections.Generic;
using System.Text;

using System.Windows;
using System.Windows.Controls;

namespace LanguageLearningApp.Views
{
    public partial class MainMenuView : UserControl
    {
        private MainWindow mainWindow;

        public MainMenuView(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window;
        }

        private void Sets_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.Navigate(new SetsView(mainWindow));
        }

        private void Games_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.Navigate(new GamesView(mainWindow));
        }
    }
}