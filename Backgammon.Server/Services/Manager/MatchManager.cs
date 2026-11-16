using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Services.Data;
using Backgammon.Server.SupportServices;
using Backgammon.Server.SupportServices.Logging;

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
        private const string MatchIdentifiersRequired = "Room and match identifiers are required.";
        private const string MatchUnavailable = "The requested match is unavailable.";
        private const string PlayerNotInMatch = "The player does not belong to this match.";


        public static MatchSession CreateMatch(int requestPlayerId, string roomId)
        {
            LobbySession lobbySession = LobbyManager.FindSavedLobbySession(roomId);
            MatchSession matchCopy;

            lock (lobbySession.Queue)
            {
                LobbyManager.ValidateHostMatch(requestPlayerId, lobbySession);
                Lobby lobby = lobbySession.RoomData;

                if (!lobby.IsFull)
                {
                    throw new KeyNotFoundException(GuestRequired);
                }
                MatchSession matchSession;

                try
                {
                    matchSession = FindSavedMatch(roomId);
                }
                catch (KeyNotFoundException matchException)
                {
                    ApplicationLogger.LogDebug("No saved match was found for room {0}. " +
                        "A new match will be created. Reason: {1}", roomId, matchException.Message);

                    matchSession = BuildMatchSession(lobby);
                    SaveMatch(matchSession);
                }
                ValidateMatchPlayers(matchSession, lobby);
                matchCopy = CopyMatch(matchSession);
            }

            return matchCopy;
        }

        public static IPreinitiationCallback GetGuestCallback(MatchSession matchSession)
        {
            if (matchSession == null)
            {
                throw new ArgumentNullException(nameof(matchSession));
            }

            LobbySession lobbySession = LobbyManager.FindSavedLobbySession(matchSession.RoomId);

            lock (lobbySession.Queue)
            {
                LobbyManager.ValidateLobbyForMatch(lobbySession);

                MatchSession savedMatch = FindSavedMatch(matchSession.RoomId);

                if (savedMatch.MatchId != matchSession.MatchId)
                {
                    throw new KeyNotFoundException(MatchUnavailable);
                }

                ValidateMatchPlayers(savedMatch, lobbySession.RoomData);

                IPreinitiationCallback guestCallback = lobbySession.GuestCallback;

                if (guestCallback == null)
                {
                    throw new KeyNotFoundException("The guest callback is unavailable.");
                }

                return guestCallback;
            }
        }

        public static MatchSession JoinMatch(int requestPlayerId, string roomId, string matchId)
        {
            if (string.IsNullOrWhiteSpace(roomId) || string.IsNullOrWhiteSpace(matchId))
            {
                throw new ArgumentException(MatchIdentifiersRequired);
            }

            LobbySession lobbySession = LobbyManager.FindSavedLobbySession(roomId);
            MatchSession matchCopy;

            lock (lobbySession.Queue)
            {
                LobbyManager.ValidateLobbyForMatch(lobbySession);

                MatchSession matchSession = FindSavedMatch(roomId);

                if (matchSession.MatchId != matchId)
                {
                    throw new KeyNotFoundException(MatchUnavailable);
                }

                bool isHost = matchSession.PlayerHostId == requestPlayerId;
                bool isGuest = matchSession.PlayerGuestId == requestPlayerId;

                if (!isHost && !isGuest)
                {
                    throw new UnauthorizedAccessException(PlayerNotInMatch);
                }

                matchCopy = CopyMatch(matchSession);
            }

            return matchCopy;
        }

        internal static bool RemoveMatch(string roomId)
        {
            bool removedMatch= ActiveMatches.TryRemove(roomId, out _);

            return removedMatch;
        }

        private static MatchSession BuildMatchSession(Lobby lobby)
        {
            var matchSession = new MatchSession
            {
                MatchId = Guid.NewGuid().ToString(),
                RoomId = lobby.RoomId,
                PlayerHostId = lobby.HostPlayerId,
                PlayerGuestId = lobby.GuestPlayerId
            };

            return matchSession;
        }

        private static void ValidateMatchPlayers(MatchSession matchSession, Lobby lobby)
        {
            bool sameHost = matchSession.PlayerHostId == lobby.HostPlayerId;
            bool sameGuest = matchSession.PlayerGuestId == lobby.GuestPlayerId;

            if (!sameHost || !sameGuest)
            {
                throw new ArgumentException(MatchAlreadyActive);
            }
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

        private static MatchSession FindSavedMatch(string roomId)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                throw new ArgumentException(MatchIdentifiersRequired, nameof(roomId));
            }

            bool matchFound = ActiveMatches.TryGetValue(roomId, out MatchSession savedMatch);

            if (!matchFound || savedMatch == null)
            {
                throw new KeyNotFoundException(MatchUnavailable);
            }

            return savedMatch;
        }

        private static void SaveMatch(MatchSession matchSession)
        {
            if (matchSession == null)
            {
                throw new ArgumentNullException(nameof(matchSession));
            }

            if (string.IsNullOrWhiteSpace(matchSession.RoomId))
            {
                throw new ArgumentException(MatchIdentifiersRequired, nameof(matchSession));
            }

            bool matchAdded = ActiveMatches.TryAdd(matchSession.RoomId, matchSession);

            if (!matchAdded)
            {
                throw new ArgumentException(MatchAlreadyActive);
            }
        }
    }
}
