using System;
using System.Collections.Concurrent;

using CurrentLobby = Backgammon.Client.PreinitiationReference.Lobby;

namespace Backgammon.Client.Controllers.Lobby
{
    internal static class LobbyContext
    {
        private static readonly ConcurrentDictionary<string, string> PendingMatchIds =
            new ConcurrentDictionary<string, string>();

        private static CurrentLobby _currentLobby;

        public static event Action LobbyUpdated;

        public static event Action<bool> LobbyClosed;

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

        public static void CloseLobby(bool wasKicked)
        {
            Action<bool> lobbyClosedHandler = LobbyClosed;

            if (lobbyClosedHandler != null)
            {
                lobbyClosedHandler.Invoke(wasKicked);
            }
        }

        public static void NotifyMatchStarted(string roomId, string matchId)
        {
            PendingMatchIds[roomId] = matchId;
            LobbyChanged();
        }

        public static bool TryTakeMatchNotice(string roomId, out string matchId)
        {
            bool noticeFound = PendingMatchIds.TryRemove(roomId, out matchId);

            if (!noticeFound)
            {
                matchId = string.Empty;
            }

            return noticeFound;
        }

        public static void ClearLobby()
        {
            CurrentLobby lobby = _currentLobby;

            if (lobby != null)
            {
                PendingMatchIds.TryRemove(lobby.RoomId, out _);
            }

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
