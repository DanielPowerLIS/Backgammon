

using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Services.Data;
using Backgammon.Server.SupportServices;
using Backgammon.Server.SupportServices.Logging;
using System;
using System.Collections.Concurrent;

namespace Backgammon.Server.Services.Manager
{
    internal static class MatchManager
    {

        private static readonly ConcurrentDictionary<string, MatchSession> ActiveMatches =
            new ConcurrentDictionary<string, MatchSession>();
        private static readonly IApplicationLogger ApplicationLogger = 
            new Log4NetApplicationLogger(typeof(MatchManager));

        private const string GuestRequired = "A guest is required to start the match.";
        private const string MatchAlreadyActive = "The lobby already has an active match.";


        public static MatchSession InitializeMatch(
            int requestPlayerId, string roomId, out IPreinitiationCallback guestCallback)
        {
            LobbySession lobbySession = LobbyManager.GetLobbySession(roomId);
            MatchSession matchCopy;

            lock (lobbySession.Queue)
            {
                LobbyManager.ValidateHostMatch(requestPlayerId, lobbySession);

                Lobby lobby = lobbySession.RoomData;

                if (!lobby.IsFull)
                {
                    throw new InvalidOperationException(GuestRequired);
                }

                guestCallback = lobbySession.GuestCallback;

                if (ActiveMatches.TryGetValue(lobby.RoomId, out MatchSession activeMatch))
                {
                    if (activeMatch.PlayerHostId != lobby.HostPlayerId || activeMatch.PlayerGuestId != lobby.GuestPlayerId)
                    {
                        throw new InvalidOperationException(MatchAlreadyActive);
                    }

                    MatchSession activeMatchCopy = CopyMatch(activeMatch);
                    return activeMatchCopy;
                }

                var matchSession = new MatchSession
                {
                    MatchId = Guid.NewGuid().ToString(),
                    RoomId = roomId,
                    PlayerHostId = lobby.HostPlayerId,
                    PlayerGuestId = lobby.GuestPlayerId
                };

                matchCopy = CopyMatch(matchSession);

                bool matchAdded = ActiveMatches.TryAdd(lobby.RoomId, matchSession);

                if (!matchAdded)
                {
                    throw new InvalidOperationException(MatchAlreadyActive);
                }
            }

            return matchCopy;
        }

        internal static void RemoveMatch(string roomId)
        { 
            ActiveMatches.TryRemove(roomId, out _);
        }


        private static MatchSession CopyMatch(MatchSession matchSession) 
        {
            var matchCopy = new MatchSession
            {
                MatchId = matchSession.MatchId,
                RoomId = matchSession.RoomId,
                PlayerHostId = matchSession.PlayerHostId,
                PlayerGuestId = matchSession.PlayerGuestId
            };

            return matchCopy;
        }

        public static MatchSession JoinMatch(int requestPlayerId, string roomId, string matchId)
        {
            if (string.IsNullOrWhiteSpace(roomId) || string.IsNullOrWhiteSpace(matchId))
            {
                throw new ArgumentException("Room and match identifiers are required.");
            }

            LobbySession lobbySession = LobbyManager.GetLobbySession(roomId);
            MatchSession matchCopy;

            lock (lobbySession.Queue)
            {
                LobbyManager.ValidateLobbyIsActive(lobbySession);

                bool matchFound = ActiveMatches.TryGetValue(roomId, out MatchSession matchSession);

                if (!matchFound || matchSession.MatchId != matchId)
                {
                    throw new InvalidOperationException("The requested match is unavailable.");
                }

                bool isHost = matchSession.PlayerHostId == requestPlayerId;
                bool isGuest = matchSession.PlayerGuestId == requestPlayerId;

                if (!isHost && !isGuest)
                {
                    throw new UnauthorizedAccessException("The player does not belong to this match.");
                }

                matchCopy = CopyMatch(matchSession);
            }

            return matchCopy;
        }
    }
}
