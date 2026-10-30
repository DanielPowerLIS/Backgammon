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
            CurrentLobby createdLobby = _serviceClient.CreateLobby(sessionId);

            return createdLobby;
        }

        public async Task<CurrentLobby> JoinLobbyAsync(string sessionId, string roomId)
        {
            CurrentLobby joinedLobby = await _serviceClient.JoinLobbyAsync(sessionId, roomId);

            return joinedLobby;
        }
    }
}
