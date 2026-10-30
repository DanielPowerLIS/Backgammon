using System;
using System.ServiceModel;

using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Services.Data;
using Backgammon.Server.SupportServices;

namespace Backgammon.Server.Services
{

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class PreinitiationService : IPreinitiationService
    {
        private readonly PlayerSessionService _playerSessionService = new PlayerSessionService();

        private static readonly string JoinLobbyErrorCode = "LobbyJoinFailed";

        public Lobby CreateLobby(string sessionId)
        {

            int userId = GetUserIdFromSession(sessionId);

            IPreinitiationCallback callback = OperationContext.Current.GetCallbackChannel<IPreinitiationCallback>();
            
            Lobby lobby = LobbyManager.CreateRoom(userId, callback);
            
            return lobby;
        }

        public bool JoinLobby(string sessionId, string roomId)
        {
            int userId = GetUserIdFromSession(sessionId);

            IPreinitiationCallback guestCallback = OperationContext.Current.GetCallbackChannel<IPreinitiationCallback>();

            try
            {
                LobbySession session = LobbyManager.JoinRoom(userId, roomId, guestCallback);
                session.HostCallback.OnLobbyUpdated(session.RoomData);

                return true;
            }
            catch (InvalidOperationException ex)
            {
                var fault = new PreinitiationFault
                {
                    ErrorCode = JoinLobbyErrorCode
                };

                throw new FaultException<PreinitiationFault>(fault);
            }
        }

        private int GetUserIdFromSession(string sessionId)
        {
            int userId = _playerSessionService.GetSession(sessionId, true).PlayerId;

            return userId;
        }
    }
}
