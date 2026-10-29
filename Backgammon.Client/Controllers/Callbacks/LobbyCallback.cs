using Backgammon.Client.LobbyReference;
using WcfLobby = Backgammon.Client.LobbyReference.Lobby;
using System;

namespace Backgammon.Client.Controllers.Callbacks
{
    internal class LobbyCallback : ILobbyServiceCallback
    {
        public event Action<WcfLobby> RoomUpdated;
        public void OnLobbyUpdated(WcfLobby lobby)
        {
            if (RoomUpdated != null) 
            {
                RoomUpdated.Invoke(lobby);
            }
        }
    }
}
