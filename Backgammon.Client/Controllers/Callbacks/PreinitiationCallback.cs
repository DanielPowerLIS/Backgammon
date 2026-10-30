using System;

using Backgammon.Client.PreinitiationReference;
using WcfLobby = Backgammon.Client.PreinitiationReference.Lobby;

namespace Backgammon.Client.Controllers.Callbacks
{
    internal class PreinitiationCallback : IPreinitiationServiceCallback
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
