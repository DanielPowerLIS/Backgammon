using System.ServiceModel;
using System.Threading.Tasks;

using Backgammon.Client.PreinitiationReference;
using CurrentLobby = Backgammon.Client.PreinitiationReference.Lobby;

namespace Backgammon.Client.Comunication
{
    internal class PreinitiationClient
    {
        private readonly PreinitiationServiceClient _serviceClient;

        public PreinitiationClient(InstanceContext callbackContext) 
        {
            _serviceClient = new PreinitiationServiceClient(callbackContext);
        }

        public CurrentLobby CreateLobby(string sessionId)
        {
            try
            {
                CurrentLobby createdLobby = _serviceClient.CreateLobby(sessionId);

                return createdLobby;
            }
            finally
            {
                AbortFaultedChannel();
            }
        }

        public async Task<CurrentLobby> JoinLobbyAsync(string sessionId, string roomId)
        {
            try
            {
                CurrentLobby joinedLobby = await _serviceClient.JoinLobbyAsync(sessionId, roomId);

                return joinedLobby;
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
