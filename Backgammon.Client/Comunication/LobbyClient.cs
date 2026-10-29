using System.ServiceModel;

using Backgammon.Client.LobbyReference;
using WcfLobby = Backgammon.Client.LobbyReference.Lobby;

namespace Backgammon.Client.Comunication
{
    internal class LobbyClient
    {
        private readonly LobbyServiceClient _serviceClient;

        public LobbyClient(InstanceContext callbackContext) 
        {
            _serviceClient = new LobbyServiceClient(callbackContext);
        } 

        public WcfLobby CreateLobby(int idUser)
        {
            WcfLobby createdLobby = _serviceClient.CreateLobby(idUser);

            return createdLobby;
        }   

        public bool JoinLobby(int idUser, string roomId) 
        {
            bool joinResult = _serviceClient.JoinLobby(idUser, roomId);

            return joinResult;
        }
    }
}
