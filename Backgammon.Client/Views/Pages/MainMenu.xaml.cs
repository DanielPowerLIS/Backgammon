using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Backgammon.Client.Views.Pages
{
    public partial class MainMenu : Page
    {
        public MainMenu()
        {
            InitializeComponent();
        }

        private void OnPlayClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Preinitiation());
        }

        private void OnLeaderboardClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Leaderboard());
        }
        private void OnProfileClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PlayerProfile());
        }

        private void OnFriendsClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Friends());
        }

        private void OnSettingsClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Settings());
        }
        private void OnSignOutClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Home());
        }
    }
}
