using System;

using CurrentLobby = Backgammon.Client.PreinitiationReference.Lobby;

namespace Backgammon.Client.Controllers.Lobby
{
    internal static class LobbyContext
    {
        private static CurrentLobby _currentLobby;

        public static void StoreLobby(CurrentLobby lobby)
        {
            if (lobby == null)
            {
                throw new ArgumentNullException(nameof(lobby));
            }

            _currentLobby = lobby;
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
    }
}
