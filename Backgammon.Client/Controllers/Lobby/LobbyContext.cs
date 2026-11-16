using System;

using CurrentLobby = Backgammon.Client.PreinitiationReference.Lobby;

namespace Backgammon.Client.Controllers.Lobby
{
    internal static class LobbyContext
    {
        private static string _pendingRoomId = string.Empty;
        private static string _pendingMatchId = string.Empty;

        private static readonly Entrance _queue = new Entrance();

        private static CurrentLobby _currentLobby;

        public static event Action LobbyUpdated;

        public static event Action<bool> LobbyClosed;

        public static void SaveLobby(CurrentLobby lobby)
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
                throw new NullReferenceException("No Lobby has been stored.");
            }

            return _currentLobby;
        }

        public static void CloseLobby(bool wasKicked)
        {
            Action<bool> lobbyClosedHandler = LobbyClosed;

            if (lobbyClosedHandler != null)
            {
                lobbyClosedHandler.Invoke(wasKicked);
            }
        }

        public static void RegisterMatchStarted(string roomId, string matchId)
        {
            lock (_queue)
            {
                _pendingRoomId = roomId;
                _pendingMatchId = matchId;
            }

            LobbyChanged();
        }

        public static string ConsumePendingMatchId(string roomId)
        {
            lock (_queue)
            {
                bool sameRoom = string.Equals(_pendingRoomId, roomId, StringComparison.Ordinal);

                bool hasPendingMatch = !string.IsNullOrWhiteSpace(_pendingMatchId);

                if (!sameRoom || !hasPendingMatch)
                {
                    return string.Empty;
                }

                string matchId = _pendingMatchId;

                _pendingRoomId = string.Empty;
                _pendingMatchId = string.Empty;

                return matchId;
            }
        }

        public static void ClearLobby()
        {
            lock (_queue)
            {
                _pendingRoomId = string.Empty;
                _pendingMatchId = string.Empty;
                _currentLobby = null;
            }
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
