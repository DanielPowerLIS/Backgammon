using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using Backgammon.Client.Views.Game;
using Backgammon.Client.Controllers.Lobby;

namespace Backgammon.Client.Views.Pages
{
    public partial class Lobby : Page
    {
        private readonly LobbyController _lobbyController;

        public Lobby()
        {
            InitializeComponent();

            _lobbyController = new LobbyController();

            Loaded += OnPageLoaded;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            string playerUsername = _lobbyController.GetCurrentPlayerUsername();

            textBlockHostUsername.Text = playerUsername;
        }

        private void OnPlay(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new GamePage());
        }

        private void OnLeaveLobby(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MainMenu());
        }

        private void OnKickPlayer(object sender, RoutedEventArgs e)
        {

        }

        private void OnOpenChat(object sender, RoutedEventArgs e)
        {

        }
    }
}
