using System.Windows;
using System.Windows.Controls;

namespace Backgammon.Client.Views.Pages
{
    public partial class Rules : Page
    {
        public Rules()
        {
            InitializeComponent();
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MainMenu());
        }
    }
}
