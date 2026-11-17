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
            _renderedVersion = 0;

            var gameController = new GameController(CurrentMatch);
            _gameController = gameController;

            gameController.MatchStateUpdated += OnMatchStateUpdated;

            try
            {
                await gameController.LoadMatchStateAsync();

                if (!IsLoaded || !ReferenceEquals(_gameController, gameController))
                {
                    return;
                }

                _isStateLoaded = true;
                RefreshBoard();

                await gameController.RegisterPlayerReadyAsync();
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

        private async void OnPageUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
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

        private void OnRollDiceButtonClick(object sender, RoutedEventArgs e)
        {
            var diceDialog = new DiceRollDialog(CheckerColor.Blue);
            diceDialog.Owner = Window.GetWindow(this);
            diceDialog.ShowDialog();

            DiceResult result = diceDialog.Result;
        }
    }
}
