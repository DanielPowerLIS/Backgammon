using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.ServiceModel;
using System.Threading.Tasks;

using Backgammon.Client.LobbyReference;
using Backgammon.Client.MatchReference;
using Backgammon.Client.Settings.Logging;
using Backgammon.Client.Views.Dialog;
using Backgammon.Client.Views.Models;
using Backgammon.Client.Views.Game;
using Backgammon.Client.Controllers.Lobby;
using Backgammon.Client.Controllers.Match;
using Backgammon.Client.Controllers.Preinitiation;

namespace Backgammon.Client.Views.Pages
{
    public partial class Lobby : Page
    {
        private readonly LobbyController _lobbyController;
        private readonly MatchController _matchController;
        private readonly PreinitiationController _preinitiationController;

        private bool _isLobbyClosed;
        private bool _isStartingMatch;
        private Chat _chatWindow;

        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(Lobby));

        internal Lobby(PreinitiationController preinitiationController)
        {
            if (preinitiationController == null)
            {
                throw new ArgumentNullException(nameof(preinitiationController));
            }

            InitializeComponent();

            _preinitiationController = preinitiationController;
            _lobbyController = new LobbyController();
            _matchController = new MatchController();

            Loaded += OnPageLoaded;
            Unloaded += OnPageUnloaded;
        }


        private async void OnPageLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            _lobbyController.LobbyUpdated -= OnLobbyUpdated;
            _lobbyController.LobbyUpdated += OnLobbyUpdated;

            _lobbyController.LobbyClosed -= OnLobbyClosed;
            _lobbyController.LobbyClosed += OnLobbyClosed;

            _lobbyController.StartListening();

            RefreshPlayers();
            await JoinPendingMatchAsync();
        }

        private void OnPageUnloaded(object sender, RoutedEventArgs routedEventArgs)
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

                await Dispatcher.BeginInvoke(refreshAction);
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

        private async Task ClosePreinitiationConnectionAsync()
        {
            try
            {
                await Task.Run(_preinitiationController.CloseConnection);
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(communicationException,
                    "The lobby callback connection could not close normally.");
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(timeoutException,
                    "Closing the lobby callback connection exceeded the time limit.");
            }
        }

        private async Task JoinPendingMatchAsync()
        {
            if (!IsLoaded || _isLobbyClosed || _isStartingMatch || _matchController.IsLobbyHost())
            {
                return;
            }

            string roomId = _lobbyController.GetRoomCode();

            string matchId = LobbyContext.ConsumePendingMatchId(roomId);

            if (string.IsNullOrWhiteSpace(matchId))
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

                CloseChatWindow();
                IsEnabled = true;
                NavigationService.Navigate(new GamePage(matchSession, this));
            }
            catch (FaultException<MatchFault> exception)
            {
                ApplicationLogger.LogWarning(
                    "The server rejected the match join request. Code: {0}", exception.Detail.ErrorCode);

                ShowStartMatchError();
            }
            catch (NullReferenceException exception)
            {
                ApplicationLogger.LogError(exception, "The player session or lobby is unavailable.");
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
        }

        private async void OnPlay(object sender, RoutedEventArgs routedEventArgs)
        {
            if (_isStartingMatch || _isLobbyClosed || !buttonPlay.IsEnabled)
            {
                return;
            }

            _isStartingMatch = true;
            IsEnabled = false;

            try
            {
                MatchSession matchSession = await _matchController.StartMatchAsync();
                if ( _isLobbyClosed || !IsLoaded)
                {
                    return;
                }

                CloseChatWindow();
                IsEnabled = true;
                NavigationService.Navigate(new GamePage(matchSession, this));
            }
            catch (NullReferenceException nullReferenceException)
            {
                ApplicationLogger.LogError(
                    nullReferenceException, "The player session or lobby is unavailable for starting a match.");
                ShowStartMatchError();
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

        private async void OnLeaveLobby(object sender, RoutedEventArgs routedEventArgs)
        {
            IsEnabled = false;

            try
            {
                await _lobbyController.LeaveLobbyAsync();

                _isLobbyClosed = true;
                _lobbyController.StopListening();

                CloseChatWindow();

                await ClosePreinitiationConnectionAsync();

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

        private async void OnLobbyClosed(bool wasKicked)
        {
            if (!Dispatcher.CheckAccess())
            {
                var closeAction = new Func<bool, Task>(HandleLobbyClosedAsync);

                var closeOperation = Dispatcher.BeginInvoke(closeAction, wasKicked);

                await closeOperation;

                Task closeTask = (Task)closeOperation.Result;

                await closeTask;
                return;
            }

            await HandleLobbyClosedAsync(wasKicked);
        }

        private async Task HandleLobbyClosedAsync(bool wasKicked)
        {
            if (_isLobbyClosed || !IsLoaded)
            {
                return;
            }

            _isLobbyClosed = true;
            IsEnabled = false;

            Window owner = Window.GetWindow(this);

            _lobbyController.StopListening();

            CloseChatWindow();

            _lobbyController.ClearLobby();

            await ClosePreinitiationConnectionAsync();

            IsEnabled = true;
            NavigationService.Navigate(new MainMenu());

            ShowLobbyClosedMessage(wasKicked, owner);
        }

        private void CloseChatWindow()
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

        private void OnKickPlayer(object sender, RoutedEventArgs routedEventArgs)
        {

        }

        private void OnOpenChat(object sender, RoutedEventArgs routedEventArgs)
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
