using System;
using System.ServiceModel;
using System.Threading.Tasks;

using Backgammon.Client.GameReference;

namespace Backgammon.Client.Comunication
{
    internal class GameClient
    {
        private const string EndpointConfigurationName = "NetTcpBinding_IGameService";

        private readonly GameServiceClient _serviceClient;

        public GameClient(InstanceContext callbackContext)
        {
            if (callbackContext == null)
            {
                throw new ArgumentNullException(nameof(callbackContext));
            }

            _serviceClient = new GameServiceClient(callbackContext, EndpointConfigurationName);
        }

        public async Task<MatchState> GetMatchStateAsync(string sessionId, string roomId, string matchId)
        {
            try
            {
                MatchState matchState = await _serviceClient.GetMatchStateAsync(sessionId, roomId, matchId);

                return matchState;
            }
            finally
            {
                AbortFaultedChannel();
            }
        }

        public async Task<MatchPreparation> GetMatchPreparationAsync(string sessionId, string roomId, string matchId)
        {
            try
            {
                MatchPreparation preparation = await _serviceClient.GetMatchPreparationAsync(
                        sessionId,
                        roomId,
                        matchId);

                return preparation;
            }
            finally
            {
                AbortFaultedChannel();
            }
        }

        public async Task<MatchPreparation> RegisterPlayerReadyAsync(string sessionId, string roomId, string matchId)
        {
            try
            {
                MatchPreparation preparation = await _serviceClient.RegisterPlayerReadyAsync(
                        sessionId,
                        roomId,
                        matchId);

                return preparation;
            }
            finally
            {
                AbortFaultedChannel();
            }
        }

        public async Task<MatchPreparation> RollOpeningDieAsync(string sessionId, PlayerOpeningRoll playerRoll)
        {
            try
            {
                MatchPreparation preparation = await _serviceClient.RollOpeningDieAsync(
                        sessionId,
                        playerRoll);

                return preparation;
            }
            finally
            {
                AbortFaultedChannel();
            }
        }

        public async Task<MatchState> RollDiceAsync(string sessionId, PlayerTurn playerRoll)
        {
            try
            {
                MatchState matchState = await _serviceClient.RollDiceAsync(sessionId, playerRoll);

                return matchState;
            }
            finally
            {
                AbortFaultedChannel();
            }
        }

        public async Task<MatchState> MoveCheckerAsync(string sessionId, PlayerCheckerMove playerMove)
        {
            try
            {
                MatchState matchState = await _serviceClient.MoveCheckerAsync(sessionId, playerMove);

                return matchState;
            }
            finally
            {
                AbortFaultedChannel();
            }
        }

        public async Task<MatchState> UndoCheckerMoveAsync(string sessionId, PlayerTurn playerTurn)
        {
            try
            {
                MatchState matchState = await _serviceClient.UndoCheckerMoveAsync(sessionId, playerTurn);

                return matchState;
            }
            finally
            {
                AbortFaultedChannel();
            }
        }

        public async Task<MatchState> ConfirmTurnAsync(string sessionId, PlayerTurn playerTurn)
        {
            try
            {
                MatchState matchState = await _serviceClient.ConfirmTurnAsync(sessionId, playerTurn);

                return matchState;
            }
            finally
            {
                AbortFaultedChannel();
            }
        }

        public void Close()
        {
            try
            {
                if (_serviceClient.State != CommunicationState.Faulted)
                {
                    _serviceClient.Close();
                }
            }
            finally
            {
                if (_serviceClient.State != CommunicationState.Closed)
                {
                    _serviceClient.Abort();
                }
            }
        }

        private void AbortFaultedChannel()
        {
            if (_serviceClient.State == CommunicationState.Faulted)
            {
                _serviceClient.Abort();
            }
        }
    }
}