using System.Windows;
using System.Windows.Controls;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

using Backgammon.Client.Controllers.Game;
using Backgammon.Client.GameReference;
using Backgammon.Client.MatchReference;
using Backgammon.Client.Views.Dialog;
using Backgammon.Client.Settings.Logging;
using Backgammon.Client.Views.Game.Controls;
using Backgammon.Client.Controllers.Session;

using GameMatchFault = Backgammon.Client.GameReference.MatchFault;

namespace Backgammon.Client.Views.Game
{
    public partial class GamePage : Page
    {
        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(GamePage));

        private GameController _gameController;
        private bool _isStateLoaded;
        private long _renderedVersion;

        private MatchStartControl _matchStartControl;

        private DiceRollDialog _openingRollDialog;
        private bool _isPlayerReady;

        public MatchSession CurrentMatch { get; }

        public GamePage(MatchSession matchSession)
        {
            if (matchSession == null)
            {
                throw new ArgumentNullException(nameof(matchSession));
            }

            InitializeComponent();

            CurrentMatch = matchSession;

            Loaded += OnPageLoaded;
            Unloaded += OnPageUnloaded;
        }

        private async void OnPageLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (_gameController != null)
            {
                return;
            }

            _isStateLoaded = false;
            _isPlayerReady = false;
            _renderedVersion = 0;
            turnDiceControlGame.IsEnabled = false;
            turnDiceControlGame.SetRolling(false);

            var gameController = new GameController(CurrentMatch);
            _gameController = gameController;

            gameController.MatchStateUpdated += OnMatchStateUpdated;

            try
            {
                await LoadGameAsync(gameController);
            }
            catch (FaultException<GameMatchFault> matchException)
            {
                ApplicationLogger.LogWarning("Loading the match state was rejected. Code: {0}",
                    matchException.Detail.ErrorCode);

                ShowGameLoadError(gameController);
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(communicationException,
                    "The match state could not be loaded.");

                ShowGameLoadError(gameController);
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(timeoutException,
                    "Loading the match state exceeded the time limit.");

                ShowGameLoadError(gameController);
            }
        }

        private async Task LoadGameAsync(GameController gameController)
        {
            await gameController.LoadMatchStateAsync();

            if (!IsLoaded || !ReferenceEquals(_gameController, gameController))
            {
                return;
            }

            _isStateLoaded = true;
            RefreshBoard();

            await gameController.RegisterPlayerReadyAsync();

            if (!IsLoaded || !ReferenceEquals(_gameController, gameController))
            {
                return;
            }

            _isPlayerReady = true;
            UpdateOpeningRollDialog(gameController.CurrentState);
        }

        private async void OnPageUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            _isPlayerReady = false;

            RemoveMatchStartControl();
            CloseOpeningRollDialog();

            GameController gameController = _gameController;

            _gameController = null;
            _isStateLoaded = false;

            if (gameController == null)
            {
                return;
            }

            gameController.MatchStateUpdated -= OnMatchStateUpdated;

            try
            {
                await Task.Run(gameController.CloseConnection);
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(communicationException,
                    "The game connection could not close normally.");
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(timeoutException,
                    "Closing the game connection exceeded the time limit.");
            }
        }

        private async void OnMatchStateUpdated()
        {
            if (!Dispatcher.CheckAccess())
            {
                var refreshAction = new Action(RefreshBoard);

                try
                {
                    await Dispatcher.InvokeAsync(refreshAction);
                }
                catch (TaskCanceledException canceledException)
                {
                    ApplicationLogger.LogDebug("The board update was canceled. Reason: {0}",
                        canceledException.Message);
                }

                return;
            }

            RefreshBoard();
        }

        private void RefreshBoard()
        {
            if (!IsLoaded || !_isStateLoaded || _gameController == null)
            {
                return;
            }

            MatchState matchState = _gameController.CurrentState;

            if (matchState.Version <= _renderedVersion)
            {
                return;
            }

            BoardState boardState = BoardState.CreateFromMatchBoard(matchState.Board);

            boardControlGame.RenderBoard(boardState);

            _renderedVersion = matchState.Version;

            UpdateTurnDice(matchState);
            UpdateMatchAnnouncement(matchState);
            UpdateOpeningRollDialog(matchState);
        }

        private void UpdateMatchAnnouncement(MatchState matchState)
        {
            if (matchState.Phase == MatchPhase.WaitingForPlayers)
            {
                MatchStartControl startControl = GetMatchStartControl();

                startControl.ShowWaiting(Properties.Resources.Message_MatchWaitingForPlayers);

                return;
            }

            if (matchState.Phase == MatchPhase.StartingCountdown)
            {
                MatchStartControl startControl = GetMatchStartControl();

                TimeSpan duration = matchState.CountdownEndsAtUtc - matchState.ServerTimeUtc;

                startControl.StartCountdown(Properties.Resources.Message_MatchStarting, duration);

                return;
            }

            RemoveMatchStartControl();
        }

        private MatchStartControl GetMatchStartControl()
        {
            if (_matchStartControl == null)
            {
                _matchStartControl = new MatchStartControl();

                contentControlMatchStart.Content = _matchStartControl;
            }

            return _matchStartControl;
        }

        private void RemoveMatchStartControl()
        {
            if (_matchStartControl == null)
            {
                return;
            }

            _matchStartControl.StopCountdown();

            contentControlMatchStart.Content = null;
            _matchStartControl = null;
        }

        private void UpdateOpeningRollDialog(MatchState matchState)
        {
            if (_openingRollDialog != null)
            {
                _openingRollDialog.ApplyMatchState(matchState);
                return;
            }

            if (!_isPlayerReady || matchState.Phase != MatchPhase.OpeningRoll)
            {
                return;
            }

            ShowOpeningRollDialog(matchState);
        }

        private void ShowOpeningRollDialog(MatchState matchState)
        {
            int playerId = PlayerSessionContext.GetSession().PlayerId;
            bool isHost = CurrentMatch.PlayerHostId == playerId;

            var diceDialog = new DiceRollDialog(isHost);
            _openingRollDialog = diceDialog;

            diceDialog.Owner = Window.GetWindow(this);
            diceDialog.RollSelected += OnOpeningRollSelected;
            diceDialog.ApplyMatchState(matchState);

            try
            {
                diceDialog.ShowDialog();
            }
            finally
            {
                diceDialog.RollSelected -= OnOpeningRollSelected;

                if (ReferenceEquals(_openingRollDialog, diceDialog))
                {
                    _openingRollDialog = null;
                }
            }
        }

        private async void OnOpeningRollSelected(int roundNumber)
        {
            GameController gameController = _gameController;
            DiceRollDialog diceDialog = _openingRollDialog;

            if (gameController == null || diceDialog == null)
            {
                return;
            }

            try
            {
                await gameController.RollOpeningDieAsync(roundNumber);

                if (ReferenceEquals(_openingRollDialog, diceDialog))
                {
                    diceDialog.CompleteRoll();
                }
            }
            catch (FaultException<GameMatchFault> matchException)
            {
                ApplicationLogger.LogWarning("The opening roll was rejected. Code: {0}",
                    matchException.Detail.ErrorCode);
                ShowOpeningRollError(diceDialog);
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(communicationException, "The opening roll could not be completed.");
                ShowOpeningRollError(diceDialog);
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(timeoutException, "The opening roll exceeded the time limit.");
                ShowOpeningRollError(diceDialog);
            }
        }

        private void ShowOpeningRollError(DiceRollDialog diceDialog)
        {
            if (IsLoaded && ReferenceEquals(_openingRollDialog, diceDialog))
            {
                diceDialog.ShowRollError();
            }
        }

        private void CloseOpeningRollDialog()
        {
            DiceRollDialog diceDialog = _openingRollDialog;

            if (diceDialog == null)
            {
                return;
            }

            _openingRollDialog = null;

            diceDialog.RollSelected -= OnOpeningRollSelected;
            diceDialog.CloseForNavigation();
        }

        private void UpdateTurnDice(MatchState matchState)
        {
            int playerId = PlayerSessionContext.GetSession().PlayerId;

            bool isLocalTurn = matchState.CurrentPlayerId == playerId;

            CheckerColor playerColor = CheckerColor.None;

            if (matchState.CurrentPlayerId == CurrentMatch.PlayerHostId)
            {
                playerColor = CheckerColor.Blue;
            }
            else if (matchState.CurrentPlayerId == CurrentMatch.PlayerGuestId)
            {
                playerColor = CheckerColor.Pink;
            }

            turnDiceControlGame.ApplyMatchState(matchState, isLocalTurn, playerColor);
            turnDiceControlGame.IsEnabled = true;

            textBlockTurnStatus.Visibility = Visibility.Collapsed;

            if (matchState.Phase == MatchPhase.WaitingForRoll || matchState.Phase == MatchPhase.MovingCheckers)
            {
                string turnMessage = Properties.Resources.Message_OpponentTurn;

                if (isLocalTurn)
                {
                    turnMessage = Properties.Resources.Message_YourTurn;
                }

                textBlockTurnStatus.Text = turnMessage;
                textBlockTurnStatus.Visibility = Visibility.Visible;
            }
        }

        private async void OnTurnRollSelected(object sender, EventArgs eventArgs)
        {
            GameController gameController = _gameController;

            if (!IsLoaded || !_isStateLoaded || gameController == null)
            {
                return;
            }

            turnDiceControlGame.SetRolling(true);

            try
            {
                await gameController.RollDiceAsync();
            }
            catch (FaultException<GameMatchFault> matchException)
            {
                ApplicationLogger.LogWarning("The turn roll was rejected. Code: {0}", matchException.Detail.ErrorCode);

                ShowTurnRollError(gameController);
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(communicationException, "The turn roll could not be completed.");

                ShowTurnRollError(gameController);
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(timeoutException, "The turn roll exceeded the time limit.");

                ShowTurnRollError(gameController);
            }
            finally
            {
                if (IsLoaded && ReferenceEquals(_gameController, gameController))
                {
                    turnDiceControlGame.SetRolling(false);
                }
            }
        }

        private void ShowTurnRollError(GameController gameController)
        {
            if (!IsLoaded || !ReferenceEquals(_gameController, gameController))
            {
                return;
            }

            var dialog = new AccountDialog( Properties.Resources.Button_RollDice,
                Properties.Resources.Message_TurnRollError);

            dialog.ShowOver(Window.GetWindow(this));
        }

        private void ShowGameLoadError(GameController gameController)
        {
            if (!IsLoaded || !ReferenceEquals(_gameController, gameController))
            {
                return;
            }

            var dialog = new AccountDialog(
                Properties.Resources.Title_ConnectionError,
                Properties.Resources.Message_GameLoadError);

            dialog.ShowOver(Window.GetWindow(this));
        }
    }
}
