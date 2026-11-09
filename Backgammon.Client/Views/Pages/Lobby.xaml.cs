using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.ServiceModel;

using Backgammon.Client.LobbyReference;
using Backgammon.Client.Settings.Logging;
using Backgammon.Client.Views.Dialog;
using Backgammon.Client.Views.Models;
using Backgammon.Client.Views.Game;
using Backgammon.Client.Controllers.Lobby;

namespace Backgammon.Client.Views.Pages
{
    public partial class Lobby : Page
    {
        private readonly LobbyController _lobbyController;

        private bool _isLobbyClosed;
        private Chat _chatWindow;

        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(Lobby));

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

            _lobbyController.LobbyClosed -= OnLobbyClosed;
            _lobbyController.LobbyClosed += OnLobbyClosed;

            _lobbyController.StartListening();

            RefreshPlayers();
        }

        private void OnPageUnloaded(object sender, RoutedEventArgs e)
        {
            _lobbyController.StopListening();

            _lobbyController.LobbyUpdated -= OnLobbyUpdated;
            _lobbyController.LobbyClosed -= OnLobbyClosed;
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
            if (!IsLoaded || _isLobbyClosed)
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

        private async void OnLeaveLobby(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;

            try
            {
                await _lobbyController.LeaveLobbyAsync();

                IsEnabled = true;

                NavigationService.Navigate(new MainMenu());
            }
            catch (FaultException<LobbyFault> lobbyException)
            {
                ApplicationLogger.LogWarning(
                    "The server rejected the lobby departure. Code: {0}",
                    lobbyException.Detail.ErrorCode);

                ShowLeaveLobbyError();
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(
                    communicationException,
                    "A communication failure prevented lobby departure from completing.");

                ShowLeaveLobbyError();
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(
                    timeoutException,
                    "The lobby departure request exceeded the time limit.");

                ShowLeaveLobbyError();
            }
            finally
            {
                IsEnabled = true;
            }
        }

        private void OnLobbyClosed(bool wasKicked)
        {
            bool hasThreadAccess = Dispatcher.CheckAccess();

            if (!hasThreadAccess)
            {
                var closeAction = new Action<bool>(HandleLobbyClosed);
                Dispatcher.BeginInvoke(closeAction, wasKicked);
                return;
            }

            HandleLobbyClosed(wasKicked);
        }

        private void HandleLobbyClosed(bool wasKicked)
        {
            if (_isLobbyClosed || !IsLoaded)
            {
                return;
            }

            _isLobbyClosed = true;

            Window owner = Window.GetWindow(this);

            _lobbyController.StopListening();

            CloseOpenChat();

            _lobbyController.ClearLobby();

            IsEnabled = true;
            NavigationService.Navigate(new MainMenu());

            ShowLobbyClosedMessage(wasKicked, owner);
        }

        private void CloseOpenChat()
        {
            if (_chatWindow == null)
            {
                return;
            }

            if (_chatWindow.IsVisible)
            {
                _chatWindow.Close();
            }
        }

        private void ShowLobbyClosedMessage(bool wasKicked, Window owner)
        {
            string title = Properties.Resources.TextBlock_LobbyClosedTitle;
            string message = Properties.Resources.TextBlock_LobbyClosedMessage;

            if (wasKicked)
            {
                title = Properties.Resources.TextBlock_LobbyKickedTitle;
                message = Properties.Resources.TextBlock_LobbyKickedMessage;
            }

            var alertDialog = new AlertDialog(
                title,
                message,
                AlertType.Normal)
            {
                Owner = owner
            };

            alertDialog.ShowDialog();
        }

        private void OnKickPlayer(object sender, RoutedEventArgs e)
        {

        }

        private void OnOpenChat(object sender, RoutedEventArgs e)
        {
            _chatWindow = new Chat
            {
                Owner = Window.GetWindow(this)
            };

            _chatWindow.ShowDialog();

            _chatWindow = null;
        }

        private void ShowLeaveLobbyError()
        {
            var alertDialog = new AlertDialog(
                Properties.Resources.TextBlock_LeaveLobbyErrorTitle,
                Properties.Resources.TextBlock_LeaveLobbyErrorMessage,
                AlertType.Error)
            {
                Owner = Window.GetWindow(this)
            };

            alertDialog.ShowDialog();
        }
    }
}
