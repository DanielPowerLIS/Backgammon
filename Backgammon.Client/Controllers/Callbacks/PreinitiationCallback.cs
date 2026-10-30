using Backgammon.Client.Controllers.Lobby;
using Backgammon.Client.PreinitiationReference;
using CurrentLobby = Backgammon.Client.PreinitiationReference.Lobby;

namespace Backgammon.Client.Controllers.Callbacks
{
    internal class PreinitiationCallback : IPreinitiationServiceCallback
    {
        public void OnLobbyUpdated(CurrentLobby lobby)
        {
            LobbyContext.StoreLobby(lobby);
        }
    }
}