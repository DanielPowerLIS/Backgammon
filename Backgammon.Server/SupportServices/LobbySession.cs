using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Services.Data;
using System.Collections.Generic;

namespace Backgammon.Server.SupportServices
{
    internal class LobbySession
    {
        public Lobby RoomData { get; set; }

        public IPreinitiationCallback HostCallback { get; set; }
        
        public IPreinitiationCallback GuestCallback { get; set; }

        public IChatCallback HostChatCallback { get; set; }

        public IChatCallback GuestChatCallback { get; set; }

        public List<ChatMessage> ChatMessages { get; } = new List<ChatMessage>();
        public Entrance Queue { get; } = new Entrance();
    }
}
