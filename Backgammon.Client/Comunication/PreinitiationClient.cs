using System.ServiceModel;

using Backgammon.Client.PreinitiationReference;
using WcfLobby = Backgammon.Client.PreinitiationReference.Lobby;

namespace Backgammon.Client.Comunication
{
    internal class PreinitiationClient
    {
        private readonly PreinitiationServiceClient _serviceClient;

        public PreinitiationClient(InstanceContext callbackContext) 
        {
            _serviceClient = new PreinitiationServiceClient(callbackContext);
        } 

        public WcfLobby CreateLobby(string SessionId)
        {
            WcfLobby createdLobby = _serviceClient.CreateLobby(SessionId);

            return createdLobby;
        }   

        public bool JoinLobby(string sessionId, string roomId) 
        {
            bool joinResult = _serviceClient.JoinLobby(sessionId, roomId);

            return joinResult;
        }
    }
}
