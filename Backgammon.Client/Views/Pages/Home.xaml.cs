using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Backgammon.Client.Views.Pages;

namespace Backgammon.Client.Views.Pages
{
    public partial class Home : Page
    {
        public Home()
        {
            InitializeComponent();
        }

        private void ButtonSignIn(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Login());
        }
    }
}
