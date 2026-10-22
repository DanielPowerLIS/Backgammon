using Backgammon.Client.Views.Game;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Backgammon.Client.Views.Pages
{
    public partial class Room : Page
    {
        public Room()
        {
            InitializeComponent();
        }

        private void OnPlay(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new GamePage());
        }
    }
}
