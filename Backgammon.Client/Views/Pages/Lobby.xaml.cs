using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using Backgammon.Client.Views.Game;

namespace Backgammon.Client.Views.Pages
{
    public partial class Lobby : Page
    {
        public Lobby()
        {
            InitializeComponent();
        }

        private void OnPlay(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new GamePage());
        }
    }
}
