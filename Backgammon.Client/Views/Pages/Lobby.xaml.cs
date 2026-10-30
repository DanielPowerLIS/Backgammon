using System;
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
            Unloaded += OnPageUnloaded;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            _lobbyController.LobbyUpdated -= OnLobbyUpdated;
            _lobbyController.LobbyUpdated += OnLobbyUpdated;

            _lobbyController.StartListening();

            RefreshPlayers();
        }

        private void OnPageUnloaded(object sender, RoutedEventArgs e)
        {
            _lobbyController.StopListening();

            _lobbyController.LobbyUpdated -= OnLobbyUpdated;
        }

        private void OnLobbyUpdated()
        {
            bool hasThreadAccess = Dispatcher.CheckAccess();

            if (!hasThreadAccess)
            {
                var refreshAction = new Action(RefreshPlayers);
                Dispatcher.BeginInvoke(refreshAction);
                return;
            }

            RefreshPlayers();
        }

        private void RefreshPlayers()
        {
            if (!IsLoaded)
            {
                return;
            }

            string hostUsername = _lobbyController.GetHostUsername();
            string guestUsername = _lobbyController.GetGuestUsername();
            string roomCode = _lobbyController.GetRoomCode();

            textBlockHostUsername.Text = hostUsername;
            textBlockGuestUsername.Text = guestUsername;
            textBlockRoomCode.Text = roomCode;
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
