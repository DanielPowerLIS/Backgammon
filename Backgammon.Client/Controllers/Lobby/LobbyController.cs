using System.ServiceModel;

using Backgammon.Client.Comunication;
using Backgammon.Client.Controllers.Callbacks;

using WcfLobby = Backgammon.Client.LobbyReference.Lobby;

namespace Backgammon.Client.Controllers.Lobby
{
    internal class LobbyController
    {
        private readonly LobbyClient _lobbyClient;
        private readonly LobbyCallback _lobbyCallback;

        public LobbyController()
        {
            _lobbyCallback = new LobbyCallback();
            InstanceContext context = new InstanceContext(_lobbyCallback);
            _lobbyClient = new LobbyClient(context);
        }

        public LobbyCallback Callback
        {
            get 
            {
                return _lobbyCallback;
            }
        }

        public WcfLobby CreateLobby(int idUser) 
        {
            WcfLobby newLobby = _lobbyClient.CreateLobby(idUser);

            return newLobby;
        }

        public bool JoinLobby(int idUser, string roomId)
        {
            bool isJoined = _lobbyClient.JoinLobby(idUser, roomId);

            return isJoined;
        }

    }
}
