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

        public async Task<MatchState> RegisterPlayerReadyAsync(string sessionId, string roomId, string matchId)
        {
            try
            {
                MatchState matchState = await _serviceClient.RegisterPlayerReadyAsync(sessionId, roomId, matchId);

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