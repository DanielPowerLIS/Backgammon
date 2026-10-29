
using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Services.Data;

namespace Backgammon.Server.SupportServices
{
    internal class LobbyRoomSession
    {
        public Lobby RoomData { get; set; }

        public ILobbyCallback HostCallback { get; set; }
        
        public ILobbyCallback GuestCallback { get; set; }
    }
}
