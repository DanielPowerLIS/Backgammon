using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

using Backgammon.Client.Comunication;
using Backgammon.Client.Controllers.Callbacks;
using Backgammon.Client.Controllers.Session;
using Backgammon.Client.GameReference;

using CurrentMatchSession = Backgammon.Client.MatchReference.MatchSession;

namespace Backgammon.Client.Controllers.Game
{
    internal class GameController
    {
        private readonly GameClient _gameClient;
        private readonly GameCallback _gameCallback;

        private readonly string _roomId;
        private readonly string _matchId;
        private readonly Entrance _queue;
        private MatchPreparation _currentPreparation;
        private MatchState _currentState;
        private bool _isClosed;

        public GameController(CurrentMatchSession matchSession)
        {
            if (matchSession == null)
            {
                throw new ArgumentNullException(nameof(matchSession));
            }

            if (string.IsNullOrWhiteSpace(matchSession.RoomId) || string.IsNullOrWhiteSpace(matchSession.MatchId))
            {
                throw new ArgumentException("Room and match identifiers are required.", nameof(matchSession));
            }

            _roomId = matchSession.RoomId;
            _matchId = matchSession.MatchId;
            _queue = new Entrance();

            _currentPreparation = null;

            _gameCallback = new GameCallback();
            _gameCallback.MatchStateUpdated += ApplyMatchState;
            _gameCallback.MatchPreparationUpdated += ApplyMatchPreparation;

            var callbackContext = new InstanceContext(_gameCallback);
            _gameClient = new GameClient(callbackContext);
        }

        public event Action MatchStateUpdated;

        public event Action MatchPreparationUpdated;

        public MatchState CurrentState
        {
            get
            {
                lock (_queue)
                {
                    if (_currentState == null)
                    {
                        throw new KeyNotFoundException("The match state has not been loaded.");
                    }

                    return _currentState;
                }
            }
        }

        public MatchPreparation CurrentPreparation
        {
            get
            {
                lock (_queue)
                {
                    if (_currentPreparation == null)
                    {
                        throw new KeyNotFoundException(
                            "The match preparation has not been loaded.");
                    }

                    MatchPreparation preparation = _currentPreparation;

                    return preparation;
                }
            }
        }

        public async Task LoadMatchStateAsync()
        {
            string sessionId = PlayerSessionContext.GetSession().SessionId;

            MatchState matchState = await _gameClient.GetMatchStateAsync(sessionId, _roomId, _matchId);

            ApplyMatchState(matchState);
        }

        public async Task LoadMatchPreparationAsync()
        {
            string sessionId = PlayerSessionContext.GetSession().SessionId;

            MatchPreparation preparation = await _gameClient.GetMatchPreparationAsync(sessionId, _roomId, _matchId);

            ApplyMatchPreparation(preparation);
        }

        public async Task RegisterPlayerReadyAsync()
        {
            string sessionId = PlayerSessionContext.GetSession().SessionId;

            MatchPreparation preparation = await _gameClient.RegisterPlayerReadyAsync(sessionId, _roomId, _matchId);

            ApplyMatchPreparation(preparation);
        }

        public async Task RollOpeningDieAsync(int roundNumber)
        {
            string sessionId = PlayerSessionContext.GetSession().SessionId;

            var playerRoll = new PlayerOpeningRoll
            {
                RoomId = _roomId,
                MatchId = _matchId,
                RoundNumber = roundNumber
            };

            MatchPreparation preparation = await _gameClient.RollOpeningDieAsync(sessionId, playerRoll);

            ApplyMatchPreparation(preparation);
        }

        public async Task RollDiceAsync()
        {
            string sessionId = PlayerSessionContext.GetSession().SessionId;

            MatchState currentState = CurrentState;

            var playerRoll = new PlayerTurn
            {
                RoomId = _roomId,
                MatchId = _matchId,
                ExpectedVersion = currentState.Version
            };

            MatchState matchState = await _gameClient.RollDiceAsync(sessionId, playerRoll);

            ApplyMatchState(matchState);
        }

        public async Task MoveCheckerAsync(CheckerMove selectedMove, long expectedVersion)
        {
            if (selectedMove == null)
            {
                throw new ArgumentNullException(nameof(selectedMove));
            }

            string sessionId = PlayerSessionContext.GetSession().SessionId;

            var playerMove = new PlayerCheckerMove
            {
                RoomId = _roomId,
                MatchId = _matchId,
                ExpectedVersion = expectedVersion,
                Move = selectedMove
            };

            MatchState matchState = await _gameClient.MoveCheckerAsync(sessionId, playerMove);

            ApplyMatchState(matchState);
        }

        public async Task UndoCheckerMoveAsync(long expectedVersion)
        {
            string sessionId = PlayerSessionContext.GetSession().SessionId;

            var playerTurn = new PlayerTurn
            {
                RoomId = _roomId,
                MatchId = _matchId,
                ExpectedVersion = expectedVersion
            };

            MatchState matchState = await _gameClient.UndoCheckerMoveAsync(sessionId, playerTurn);

            ApplyMatchState(matchState);
        }

        public async Task ConfirmTurnAsync(long expectedVersion)
        {
            string sessionId = PlayerSessionContext.GetSession().SessionId;

            var playerTurn = new PlayerTurn
            {
                RoomId = _roomId,
                MatchId = _matchId,
                ExpectedVersion = expectedVersion
            };

            MatchState matchState = await _gameClient.ConfirmTurnAsync(sessionId, playerTurn);

            ApplyMatchState(matchState);
        }

        public void CloseConnection()
        {
            lock (_queue)
            {
                if (_isClosed)
                {
                    return;
                }

                _isClosed = true;
            }

            _gameCallback.MatchStateUpdated -= ApplyMatchState;
            _gameCallback.MatchPreparationUpdated -= ApplyMatchPreparation;
            _gameClient.Close();
        }

        private void ApplyMatchState(MatchState matchState)
        {
            if (matchState == null)
            {
                return;
            }

            bool sameRoom = string.Equals(matchState.RoomId, _roomId, StringComparison.Ordinal);

            bool sameMatch = string.Equals(matchState.MatchId, _matchId, StringComparison.Ordinal);

            if (!sameRoom || !sameMatch)
            {
                return;
            }

            lock (_queue)
            {
                if (_isClosed)
                {
                    return;
                }

                if (_currentState != null && matchState.Version <= _currentState.Version)
                {
                    return;
                }

                _currentState = matchState;
            }

            Action stateUpdatedHandler = MatchStateUpdated;

            if (stateUpdatedHandler != null)
            {
                stateUpdatedHandler.Invoke();
            }
        }

        private void ApplyMatchPreparation(MatchPreparation preparation)
        {
            if (preparation == null)
            {
                return;
            }

            bool sameRoom = string.Equals(preparation.RoomId, _roomId, StringComparison.Ordinal);

            bool sameMatch = string.Equals(preparation.MatchId, _matchId, StringComparison.Ordinal);

            if (!sameRoom || !sameMatch)
            {
                return;
            }

            lock (_queue)
            {
                if (_isClosed)
                {
                    return;
                }

                if (_currentPreparation != null && preparation.Version <= _currentPreparation.Version)
                {
                    return;
                }

                _currentPreparation = preparation;
            }

            Action preparationUpdatedHandler = MatchPreparationUpdated;

            if (preparationUpdatedHandler != null)
            {
                preparationUpdatedHandler.Invoke();
            }
        }
    }
}