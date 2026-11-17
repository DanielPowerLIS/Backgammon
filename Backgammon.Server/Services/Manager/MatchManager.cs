using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Services.Data;
using Backgammon.Server.SupportServices;
using Backgammon.Server.SupportServices.Logging;
using Backgammon.Server.Domain;

namespace Backgammon.Server.Services.Manager
{
    internal static class MatchManager
    {

        private static readonly ConcurrentDictionary<string, ActiveMatch> ActiveMatches =
            new ConcurrentDictionary<string, ActiveMatch>();
        private static readonly IApplicationLogger ApplicationLogger = 
            new Log4NetApplicationLogger(typeof(MatchManager));

        private const int CountdownDurationSeconds = 3;

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

        public static MatchState GetMatchState(int PlayerId, string roomId, string matchId)
        {
            if (string.IsNullOrWhiteSpace(roomId) || string.IsNullOrWhiteSpace(matchId))
            {
                throw new ArgumentException(MatchIdentifiersRequired);
            }

            ActiveMatch activeMatch = FindSavedActiveMatch(roomId);
            MatchState stateCopy;

            lock (activeMatch.Queue)
            {
                MatchSession matchSession = activeMatch.Session;

                if (matchSession.MatchId != matchId)
                {
                    throw new KeyNotFoundException(MatchUnavailable);
                }

                bool isHost = matchSession.PlayerHostId == PlayerId;

                bool isGuest = matchSession.PlayerGuestId == PlayerId;

                if (!isHost && !isGuest)
                {
                    throw new UnauthorizedAccessException(PlayerNotInMatch);
                }

                stateCopy = MatchStateFactory.Copy(activeMatch.State);
            }

            return stateCopy;
        }

        public static MatchState RegisterPlayerReady(MatchPlayer request, IGameCallback playerCallback)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            ActiveMatch activeMatch = FindSavedActiveMatch(request.RoomId);
            MatchState stateCopy;

            lock (activeMatch.Queue)
            {
                if (activeMatch.Session.MatchId != request.MatchId)
                {
                    throw new KeyNotFoundException(MatchUnavailable);
                }
                bool hostWasReady = activeMatch.HostReady;
                bool guestWasReady = activeMatch.GuestReady;

                RegisterReadyPlayer(activeMatch, request.PlayerId, playerCallback);

                bool readinessChanged = hostWasReady != activeMatch.HostReady ||
                    guestWasReady != activeMatch.GuestReady;

                bool shouldStartCountdown = activeMatch.HostReady && activeMatch.GuestReady &&
                    activeMatch.State.Phase == MatchPhase.WaitingForPlayers;

                if (shouldStartCountdown)
                {
                    activeMatch.State.Phase = MatchPhase.StartingCountdown;

                    activeMatch.State.CountdownEndsAtUtc = DateTime.UtcNow.AddSeconds(CountdownDurationSeconds);
                }

                if (readinessChanged || shouldStartCountdown)
                {
                    activeMatch.State.Version++;
                }
                stateCopy = MatchStateFactory.Copy(activeMatch.State);
            }
            return stateCopy;
        }

        public static IGameCallback[] GetGameCallbacks(MatchPlayer request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            ActiveMatch activeMatch = FindSavedActiveMatch(request.RoomId);

            var callbacks = new List<IGameCallback>();

            lock (activeMatch.Queue)
            {
                MatchSession matchSession = activeMatch.Session;

                if (matchSession.MatchId != request.MatchId)
                {
                    throw new KeyNotFoundException(MatchUnavailable);
                }

                bool isHost = matchSession.PlayerHostId == request.PlayerId;

                bool isGuest = matchSession.PlayerGuestId == request.PlayerId;

                if (!isHost && !isGuest)
                {
                    throw new UnauthorizedAccessException(PlayerNotInMatch);
                }

                if (activeMatch.HostCallback != null)
                {
                    callbacks.Add(activeMatch.HostCallback);
                }

                if (activeMatch.GuestCallback != null)
                {
                    callbacks.Add(activeMatch.GuestCallback);
                }
            }
            return callbacks.ToArray();
        }

        public static void StartCountdown(MatchPlayer request, TimerCallback countdownCallback)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (countdownCallback == null)
            {
                throw new ArgumentNullException(nameof(countdownCallback));
            }
            ActiveMatch activeMatch = FindSavedActiveMatch(request.RoomId);

            lock (activeMatch.Queue)
            {
                ActiveMatch savedMatch = FindSavedActiveMatch(request.RoomId);

                if (!ReferenceEquals(savedMatch, activeMatch) || activeMatch.Session.MatchId != request.MatchId)
                {
                    throw new KeyNotFoundException(MatchUnavailable);
                }

                if (activeMatch.State.Phase != MatchPhase.StartingCountdown || activeMatch.CountdownTimer != null)
                {
                    return;
                }
                TimeSpan remainingTime = activeMatch.State.CountdownEndsAtUtc - DateTime.UtcNow;

                if (remainingTime < TimeSpan.Zero)
                {
                    remainingTime = TimeSpan.Zero;
                }

                activeMatch.CountdownTimer = new Timer(countdownCallback, activeMatch, remainingTime,
                    Timeout.InfiniteTimeSpan);
            }
        }

        public static MatchState CompleteCountdown(ActiveMatch activeMatch)
        {
            if (activeMatch == null)
            {
                throw new ArgumentNullException(nameof(activeMatch));
            }

            MatchState stateCopy;

            lock (activeMatch.Queue)
            {
                ActiveMatch savedMatch = FindSavedActiveMatch(activeMatch.Session.RoomId);

                if (!ReferenceEquals(savedMatch, activeMatch))
                {
                    throw new KeyNotFoundException(MatchUnavailable);
                }

                if (activeMatch.State.Phase == MatchPhase.StartingCountdown)
                {
                    activeMatch.State.Phase = MatchPhase.OpeningRoll;
                    activeMatch.State.CountdownEndsAtUtc = DateTime.MinValue;
                    activeMatch.State.Version++;
                }

                stateCopy = MatchStateFactory.Copy(activeMatch.State);
            }

            return stateCopy;
        }

        internal static bool RemoveMatch(string roomId)
        {
            bool removedMatch = ActiveMatches.TryRemove(roomId, out ActiveMatch activeMatch);

            if (removedMatch)
            {
                activeMatch.StopCountdown();
            }

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

        private static void RegisterReadyPlayer(ActiveMatch activeMatch, int playerId,
            IGameCallback playerCallback)
        {
            if (playerCallback == null)
            {
                throw new ArgumentNullException(nameof(playerCallback));
            }

            MatchSession matchSession = activeMatch.Session;

            if (matchSession.PlayerHostId == playerId)
            {
                activeMatch.HostCallback = playerCallback;
                activeMatch.HostReady = true;
                return;
            }

            if (matchSession.PlayerGuestId == playerId)
            {
                activeMatch.GuestCallback = playerCallback;
                activeMatch.GuestReady = true;
                return;
            }

            throw new UnauthorizedAccessException(PlayerNotInMatch);
        }

        private static ActiveMatch FindSavedActiveMatch(string roomId)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                throw new ArgumentException(MatchIdentifiersRequired, nameof(roomId));
            }

            bool matchFound = ActiveMatches.TryGetValue(roomId, out ActiveMatch activeMatch);

            if (!matchFound || activeMatch == null)
            {
                throw new KeyNotFoundException(MatchUnavailable);
            }

            return activeMatch;
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
            ActiveMatch activeMatch = FindSavedActiveMatch(roomId);

            return activeMatch.Session;
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

            var activeMatch = new ActiveMatch(matchSession);

            bool matchAdded = ActiveMatches.TryAdd(matchSession.RoomId, activeMatch);

            if (!matchAdded)
            {
                throw new ArgumentException(MatchAlreadyActive);
            }
        }
    }
}
