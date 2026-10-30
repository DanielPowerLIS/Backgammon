using System;

using CurrentLobby = Backgammon.Client.PreinitiationReference.Lobby;

namespace Backgammon.Client.Controllers.Lobby
{
    internal static class LobbyContext
    {
        private static CurrentLobby _currentLobby;

        public static event Action LobbyUpdated;

        public static void StoreLobby(CurrentLobby lobby)
        {
            if (lobby == null)
            {
                throw new ArgumentNullException(nameof(lobby));
            }

            _currentLobby = lobby;

            LobbyChanged();
        }

        public static CurrentLobby GetLobby()
        {
            if (_currentLobby == null)
            {
                throw new InvalidOperationException(
                    "No Lobby has been stored.");
            }

            return _currentLobby;
        }

        public static void ClearLobby()
        {
            _currentLobby = null;
        }

        private static void LobbyChanged()
        {
            Action lobbyUpdatedHandler = LobbyUpdated;

            if (lobbyUpdatedHandler != null)
            {
                lobbyUpdatedHandler.Invoke();
            }
        }
    }
}
