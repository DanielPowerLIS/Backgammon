using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Services.Data;
using Backgammon.Server.SupportServices;
using System;
using System.ServiceModel;

namespace Backgammon.Server.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class LobbyService : ILobbyService
    {
        public Lobby CreateLobby(int idUser)
        {
            ILobbyCallback callback = OperationContext.Current.GetCallbackChannel<ILobbyCallback>();

            return LobbyManager.CreateRoom(idUser, callback);
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
                    ErrorCode = "LobbyJoinFailed",
                    ErrorMessage = ex.Message
                };

                throw new FaultException<LobbyFault>(fault);
            }
        }
    }
}
