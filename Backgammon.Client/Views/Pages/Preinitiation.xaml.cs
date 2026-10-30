using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using Backgammon.Client.Controllers.Preinitiation;

namespace Backgammon.Client.Views.Pages
{
    public partial class Preinitiation : Page
    {
        private readonly PreinitiationController _preinitiationController;

        public Preinitiation()
        {
            InitializeComponent();
            _preinitiationController = new PreinitiationController();
        }

        private void OnCreateLobbyClick(object sender, RoutedEventArgs e)
        {
            if (_preinitiationController.CreateLobby())
            {
                NavigationService.Navigate(new Lobby());
            }
        }

        private void OnJoinClick(object sender, RoutedEventArgs e)
        {

        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack) 
            { 
                NavigationService.GoBack();
            }
        }
    }
}
