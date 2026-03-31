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

using System.Windows;

namespace LanguageLearningApp.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MainContent.Content = new MainMenuView(this);
        }

        public void Navigate(object view)
        {
            MainContent.Content = view;
        }
    }
}