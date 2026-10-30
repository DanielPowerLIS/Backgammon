using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Services.Data;

namespace Backgammon.Server.SupportServices
{
    internal class LobbySession
    {
        public Lobby RoomData { get; set; }

        public IPreinitiationCallback HostCallback { get; set; }
        
        public IPreinitiationCallback GuestCallback { get; set; }
    }
}
