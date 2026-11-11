using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.ServiceModel;

using Backgammon.Client.LobbyReference;
using Backgammon.Client.MatchReference;
using Backgammon.Client.Settings.Logging;
using Backgammon.Client.Views.Dialog;
using Backgammon.Client.Views.Models;
using Backgammon.Client.Views.Game;
using Backgammon.Client.Controllers.Lobby;
using Backgammon.Client.Controllers.Match;
using System.Threading.Tasks;

namespace Backgammon.Client.Views.Pages
{
    public partial class Lobby : Page
    {
        private readonly LobbyController _lobbyController;
        private readonly MatchController _matchController;

        private bool _isLobbyClosed;
        private bool _isStartingMatch;
        private Chat _chatWindow;

        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(Lobby));

        public Lobby()
        {
            InitializeComponent();

            _lobbyController = new LobbyController();
            _matchController = new MatchController();

            Loaded += OnPageLoaded;
            Unloaded += OnPageUnloaded;
        }

        private async void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            _lobbyController.LobbyUpdated -= OnLobbyUpdated;
            _lobbyController.LobbyUpdated += OnLobbyUpdated;

            _lobbyController.LobbyClosed -= OnLobbyClosed;
            _lobbyController.LobbyClosed += OnLobbyClosed;

            _lobbyController.StartListening();

            RefreshPlayers();
            await JoinPendingMatchAsync();
        }

        private void OnPageUnloaded(object sender, RoutedEventArgs e)
        {
            _lobbyController.StopListening();

            _lobbyController.LobbyUpdated -= OnLobbyUpdated;
            _lobbyController.LobbyClosed -= OnLobbyClosed;
        }

        private async void OnLobbyUpdated()
        {
            bool hasThreadAccess = Dispatcher.CheckAccess();

            if (!hasThreadAccess)
            {
                var refreshAction = new Action(OnLobbyUpdated);
                _ = Dispatcher.BeginInvoke(refreshAction);
                return;
            }

            RefreshPlayers();
            await JoinPendingMatchAsync();
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

            bool isLobbyHost = _matchController.IsLobbyHost();

            buttonPlay.Visibility = Visibility.Collapsed;

            if (isLobbyHost)
            {
                buttonPlay.Visibility = Visibility.Visible;
            }

            buttonPlay.IsEnabled = _matchController.CanStartMatch() && !_isStartingMatch;
        }

        private async Task JoinPendingMatchAsync()
        {
            if (!IsLoaded || _isLobbyClosed || _isStartingMatch || _matchController.IsLobbyHost())
            {
                return;
            }

            string roomId = _lobbyController.GetRoomCode();

            if (!LobbyContext.TryTakeMatchNotice(roomId, out string matchId))
            {
                return;
            }

            _isStartingMatch = true;
            IsEnabled = false;

            try
            {
                await OpenGuestMatchAsync(roomId, matchId);
            }
            finally
            {
                _isStartingMatch = false;
                IsEnabled = true;
            }
        }

        private async Task OpenGuestMatchAsync(string roomId, string matchId)
        {
            try
            {
                MatchSession matchSession = await _matchController.JoinMatchAsync(roomId, matchId);

                if (!IsLoaded || _isLobbyClosed || _lobbyController.GetRoomCode() != roomId)
                {
                    return;
                }

                CloseOpenChat();
                IsEnabled = true;
                NavigationService.Navigate(new GamePage(matchSession));
            }
            catch (FaultException<MatchFault> exception)
            {
                ApplicationLogger.LogWarning(
                    "The server rejected the match join request. Code: {0}", exception.Detail.ErrorCode);

                ShowStartMatchError();
            }
            catch (CommunicationException exception)
            {
                ApplicationLogger.LogError(exception, "The request to join the match failed.");
                ShowStartMatchError();
            }
            catch (TimeoutException exception)
            {
                ApplicationLogger.LogError(exception, "Joining the match timed out.");
                ShowStartMatchError();
            }
            catch (InvalidOperationException exception)
            {
                ApplicationLogger.LogError(exception, "The player session or lobby is unavailable.");
                ShowStartMatchError();
            }
        }

        private async void OnPlay(object sender, RoutedEventArgs e)
        {
            if (_isStartingMatch || _isLobbyClosed || !buttonPlay.IsEnabled)
            {
                return;
            }

            _isStartingMatch = true;
            IsEnabled = false;

            try
            {
                MatchSession matchSession = await _matchController.InitializeMatchAsync();
                if ( _isLobbyClosed || !IsLoaded)
                {
                    return;
                }

                CloseOpenChat();
                IsEnabled = true;
                NavigationService.Navigate(new GamePage(matchSession));
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(
                    communicationException, "The request to initialize the match failed.");

                ShowStartMatchError();
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(
                    timeoutException, "Initializing the match exceeded the time limit.");

                ShowStartMatchError();
            }
            catch (InvalidOperationException contextException)
            {
                ApplicationLogger.LogError(
                    contextException, "The player session or lobby is unavailable for starting a match.");

                ShowStartMatchError();
            }
            finally
            {
                _isStartingMatch = false;
                IsEnabled = true;
                RefreshPlayers();
            }

        }

        private void ShowStartMatchError()
        {
            var alertDialog = new AlertDialog(
                Properties.Resources.TextBlock_LobbyStartErrorTitle,
                Properties.Resources.TextBlock_LobbyStartErrorMessage,
                AlertType.Error)
            {
                Owner = Window.GetWindow(this)
            };

            alertDialog.ShowDialog();
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
