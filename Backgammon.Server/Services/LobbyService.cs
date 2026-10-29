using System;
using System.ServiceModel;

using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Services.Data;
using Backgammon.Server.SupportServices;

namespace Backgammon.Server.Services
{

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class LobbyService : ILobbyService
    {

        private static readonly string JoinLobbyErrorCode = "LobbyJoinFailed";

        public Lobby CreateLobby(int idUser)
        {
            ILobbyCallback callback = OperationContext.Current.GetCallbackChannel<ILobbyCallback>();

            
            Lobby lobby = LobbyManager.CreateRoom(idUser, callback);
            
            return lobby;
        }

        public bool JoinLobby(int idUser, string roomId)
        {
            ILobbyCallback guestCallback = OperationContext.Current.GetCallbackChannel<ILobbyCallback>();

            try
            {
                LobbyRoomSession session = LobbyManager.JoinRoom(idUser, roomId, guestCallback);
                session.HostCallback.OnLobbyUpdated(session.RoomData);

                return true;
            }
            catch (InvalidOperationException ex)
            {
                var fault = new LobbyFault
                {
                    ErrorCode = JoinLobbyErrorCode
                };

                throw new FaultException<LobbyFault>(fault);
            }
        }
    }
}
