using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Backgammon.Client.Views.Pages
{
    public partial class Preinitiation : Page
    {
        public Preinitiation()
        {
            InitializeComponent();
        }

        private void OnCreateLobbyClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Lobby());
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
