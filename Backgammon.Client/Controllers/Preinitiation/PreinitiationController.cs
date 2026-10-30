using System.ServiceModel;

using Backgammon.Client.Comunication;
using Backgammon.Client.Controllers.Callbacks;
using Backgammon.Client.Controllers.Lobby;
using Backgammon.Client.Controllers.Session;
using CurrentLobby = Backgammon.Client.PreinitiationReference.Lobby;

namespace Backgammon.Client.Controllers.Preinitiation
{
    internal class PreinitiationController
    {
        private readonly PreinitiationClient _preinitiationClient;
        private readonly PreinitiationCallback _preinitiationCallback;

        public PreinitiationController()
        {
            _preinitiationCallback = new PreinitiationCallback();
            InstanceContext context = new InstanceContext(_preinitiationCallback);
            _preinitiationClient = new PreinitiationClient(context);
        }

        public PreinitiationCallback Callback
        {
            get 
            {
                return _preinitiationCallback;
            }
        }

        public bool CreateLobby() 
        {
            bool lobbyCreated = false;

            string sessionId = PlayerSessionContext.GetSession().SessionId;

            CurrentLobby newLobby = _preinitiationClient.CreateLobby(sessionId);

            LobbyContext.StoreLobby(newLobby);

            lobbyCreated = true;

            return lobbyCreated;
        }

        public bool JoinLobby(string roomId)
        {
            string sessionId = PlayerSessionContext.GetSession().SessionId;

            bool isJoined = _preinitiationClient.JoinLobby(sessionId, roomId);

            return isJoined;
        }

    }
}
