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

            _gameCallback = new GameCallback();
            _gameCallback.MatchStateUpdated += ApplyMatchState;

            var callbackContext = new InstanceContext(_gameCallback);
            _gameClient = new GameClient(callbackContext);
        }

        public event Action MatchStateUpdated;

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

        public async Task LoadMatchStateAsync()
        {
            string sessionId = PlayerSessionContext.GetSession().SessionId;

            MatchState matchState = await _gameClient.GetMatchStateAsync(sessionId, _roomId, _matchId);

            ApplyMatchState(matchState);
        }

        public async Task RegisterPlayerReadyAsync()
        {
            string sessionId = PlayerSessionContext.GetSession().SessionId;

            MatchState matchState = await _gameClient.RegisterPlayerReadyAsync(sessionId, _roomId, _matchId);

            ApplyMatchState(matchState);
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

            MatchState matchState = await _gameClient.RollOpeningDieAsync(sessionId, playerRoll);

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
    }
}