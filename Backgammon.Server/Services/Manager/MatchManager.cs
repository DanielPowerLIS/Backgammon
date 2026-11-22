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
        private const int NoDieValue = 0;
        private const int MinimumTurnTimerMilliseconds = 1;

        private static readonly string GuestRequired = "A guest is required to start the match.";
        private static readonly string MatchAlreadyActive = "The lobby already has an active match.";
        private static readonly string MatchIdentifiersRequired = "Room and match identifiers are required.";
        private static readonly string MatchUnavailable = "The requested match is unavailable.";
        private static readonly string PlayerNotInMatch = "The player does not belong to this match.";
        private static readonly string OpeningRollUnavailable = "The match is not in the opening roll phase.";
        private static readonly string OpeningRollRoundUnavailable = "The opening roll round is unavailable.";
        private static readonly string OpeningDieAlreadyRolled = "The player has already rolled in this round.";


        public static MatchSession CreateMatch(int PlayerId, string roomId)
        {
            LobbySession lobbySession = LobbyManager.FindSavedLobbySession(roomId);
            MatchSession matchCopy;

            lock (lobbySession.Queue)
            {
                LobbyManager.ValidateHostMatch(PlayerId, lobbySession);
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
                activeMatch.Clock.UpdateState();

                CompleteExpiredMatch(activeMatch);

                activeMatch.Clock.UpdateState();

                stateCopy = MatchStateFactory.Copy(activeMatch.State);
            }

            return stateCopy;
        }

        public static MatchPreparation GetMatchPreparation(MatchPlayer player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            ActiveMatch activeMatch = FindSavedActiveMatch(player.RoomId);

            lock (activeMatch.Queue)
            {
                ValidateMatchPlayer(activeMatch, player);

                MatchPreparation preparationCopy = MatchPreparationFactory.Copy(activeMatch.Preparation);

                return preparationCopy;
            }
        }

        public static MatchPreparation RegisterPlayerReady(MatchPlayer player, IGameCallback playerCallback)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            ActiveMatch activeMatch = FindSavedActiveMatch(player.RoomId);

            lock (activeMatch.Queue)
            {
                ValidateMatchPlayer(activeMatch, player);

                bool hostWasReady = activeMatch.HostReady;
                bool guestWasReady = activeMatch.GuestReady;

                RegisterReadyPlayer(activeMatch, player.PlayerId, playerCallback);

                MatchPreparation preparation = activeMatch.Preparation;

                bool readinessChanged = hostWasReady != activeMatch.HostReady ||
                    guestWasReady != activeMatch.GuestReady;

                bool shouldStartCountdown = activeMatch.HostReady && activeMatch.GuestReady &&
                    preparation.Phase == MatchPhase.WaitingForPlayers;

                if (shouldStartCountdown)
                {
                    preparation.Phase = MatchPhase.StartingCountdown;
                    preparation.CountdownEndsAtUtc = DateTime.UtcNow.AddSeconds(CountdownDurationSeconds);
                }

                if (readinessChanged || shouldStartCountdown)
                {
                    preparation.Version++;
                }

                MatchPreparation preparationCopy = MatchPreparationFactory.Copy(preparation);

                return preparationCopy;
            }
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

        public static void StartCountdown(MatchPlayer player, TimerCallback countdownCallback)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (countdownCallback == null)
            {
                throw new ArgumentNullException(nameof(countdownCallback));
            }

            ActiveMatch activeMatch = FindSavedActiveMatch(player.RoomId);

            lock (activeMatch.Queue)
            {
                ValidateMatchPlayer(activeMatch, player);

                MatchPreparation preparation = activeMatch.Preparation;

                if (preparation.Phase != MatchPhase.StartingCountdown || activeMatch.CountdownTimer != null)
                {
                    return;
                }

                TimeSpan remainingTime = preparation.CountdownEndsAtUtc - DateTime.UtcNow;

                if (remainingTime < TimeSpan.Zero)
                {
                    remainingTime = TimeSpan.Zero;
                }

                activeMatch.CountdownTimer = new Timer(
                    countdownCallback,
                    activeMatch,
                    remainingTime,
                    Timeout.InfiniteTimeSpan);
            }
        }

        public static void StartTurnTimer(MatchPlayer player, TimerCallback timeoutCallback)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (timeoutCallback == null)
            {
                throw new ArgumentNullException(nameof(timeoutCallback));
            }

            ActiveMatch activeMatch = FindSavedActiveMatch(player.RoomId);

            lock (activeMatch.Queue)
            {
                ValidateMatchPlayer(activeMatch, player);

                if (!activeMatch.State.Clock.IsRunning)
                {
                    return;
                }

                TimeSpan dueTime = GetTurnTimerDelay(activeMatch.Clock);

                if (activeMatch.TurnTimer == null)
                {
                    activeMatch.TurnTimer = new Timer(timeoutCallback, activeMatch, dueTime, Timeout.InfiniteTimeSpan);
                }
                else
                {
                    activeMatch.TurnTimer.Change(dueTime, Timeout.InfiniteTimeSpan);
                }
            }
        }

        public static MatchPreparation CompleteCountdown(ActiveMatch activeMatch)
        {
            if (activeMatch == null)
            {
                throw new ArgumentNullException(nameof(activeMatch));
            }

            lock (activeMatch.Queue)
            {
                ActiveMatch savedMatch = FindSavedActiveMatch(activeMatch.Session.RoomId);

                if (!ReferenceEquals(savedMatch, activeMatch))
                {
                    throw new KeyNotFoundException(MatchUnavailable);
                }

                MatchPreparation preparation = activeMatch.Preparation;

                if (preparation.Phase == MatchPhase.StartingCountdown)
                {
                    preparation.Phase = MatchPhase.OpeningRoll;
                    preparation.CountdownEndsAtUtc = DateTime.MinValue;
                    preparation.Version++;
                }

                MatchPreparation preparationCopy = MatchPreparationFactory.Copy(preparation);

                return preparationCopy;
            }
        }

        public static MatchPreparation RollOpeningDie(MatchPlayer player, int roundNumber)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            ActiveMatch activeMatch = FindSavedActiveMatch(player.RoomId);

            lock (activeMatch.Queue)
            {
                ValidateMatchPlayer(activeMatch, player);

                MatchPreparation preparation = activeMatch.Preparation;

                ValidateOpeningRollPhase(preparation);

                MatchOpeningRollState openingRoll = GetOpeningRoll(preparation.OpeningRoll, roundNumber);

                RollPlayerDie(activeMatch.Session, openingRoll, player.PlayerId);

                preparation.OpeningRoll = openingRoll;

                CompleteOpeningRoll(activeMatch);

                preparation.Version++;

                MatchPreparation preparationCopy = MatchPreparationFactory.Copy(preparation);

                return preparationCopy;
            }
        }

        public static MatchState RollDice(MatchPlayer player, long expectedVersion)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            ActiveMatch activeMatch = FindSavedActiveMatch(player.RoomId);

            lock (activeMatch.Queue)
            {
                ValidateMatchPlayer(activeMatch, player);

                if (CompleteExpiredMatch(activeMatch))
                {
                    MatchState finishedState = MatchStateFactory.Copy(activeMatch.State);

                    return finishedState;
                }

                ValidateMatchVersion(activeMatch.State, expectedVersion);

                CheckerColor playerColor = GetPlayerColor(activeMatch.Session, player.PlayerId);
                MatchTurn.RollDice(activeMatch.State, player.PlayerId, playerColor);

                activeMatch.TurnHistory.Clear();

                activeMatch.State.Turn.CanUndo = activeMatch.TurnHistory.HasSnapshots;

                activeMatch.State.Version++;
                activeMatch.Clock.UpdateState();

                MatchState stateCopy = MatchStateFactory.Copy(activeMatch.State);

                return stateCopy;
            }
        }

        public static MatchState MoveChecker(MatchPlayer player, CheckerMove selectedMove, long expectedVersion)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            ActiveMatch activeMatch = FindSavedActiveMatch(player.RoomId);

            lock (activeMatch.Queue)
            {
                ValidateMatchPlayer(activeMatch, player);

                if (CompleteExpiredMatch(activeMatch))
                {
                    MatchState finishedState = MatchStateFactory.Copy(activeMatch.State);

                    return finishedState;
                }

                ValidateMatchVersion(activeMatch.State, expectedVersion);

                MatchTurn.ValidatePlayerTurn(activeMatch.State, player.PlayerId);

                CheckerColor playerColor = GetPlayerColor(
                    activeMatch.Session,
                    player.PlayerId);

                TurnSnapshot snapshot = MatchStateFactory.CreateTurnSnapshot(activeMatch.State);

                MatchTurn.MoveChecker(activeMatch.State, playerColor, selectedMove);

                activeMatch.TurnHistory.Save(snapshot);

                activeMatch.State.Turn.CanUndo = activeMatch.TurnHistory.HasSnapshots;

                activeMatch.State.Version++;
                activeMatch.Clock.UpdateState();

                MatchState stateCopy = MatchStateFactory.Copy(activeMatch.State);

                return stateCopy;
            }
        }

        public static MatchState UndoCheckerMove(MatchPlayer player, long expectedVersion)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            ActiveMatch activeMatch = FindSavedActiveMatch(player.RoomId);

            lock (activeMatch.Queue)
            {
                ValidateMatchPlayer(activeMatch, player);

                if (CompleteExpiredMatch(activeMatch))
                {
                    MatchState finishedState = MatchStateFactory.Copy(activeMatch.State);

                    return finishedState;
                }

                ValidateMatchVersion(activeMatch.State, expectedVersion);

                MatchTurn.ValidatePlayerTurn(activeMatch.State, player.PlayerId);

                if (!activeMatch.TurnHistory.HasSnapshots)
                {
                    throw new ArgumentException("There are no checker movements to undo.");
                }

                CheckerColor playerColor = GetPlayerColor(
                    activeMatch.Session,
                    player.PlayerId);

                TurnSnapshot snapshot = activeMatch.TurnHistory.GetLastSnapshot();

                MatchTurn.UndoCheckerMove(activeMatch.State, snapshot, playerColor);

                activeMatch.TurnHistory.TakeLastSnapshot();

                activeMatch.State.Turn.CanUndo = activeMatch.TurnHistory.HasSnapshots;

                activeMatch.State.Version++;
                activeMatch.Clock.UpdateState();

                MatchState stateCopy = MatchStateFactory.Copy(activeMatch.State);

                return stateCopy;
            }
        }

        public static MatchState ConfirmTurn(MatchPlayer player, long expectedVersion)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            ActiveMatch activeMatch = FindSavedActiveMatch(player.RoomId);

            lock (activeMatch.Queue)
            {
                ValidateMatchPlayer(activeMatch, player);

                if (CompleteExpiredMatch(activeMatch))
                {
                    MatchState finishedState = MatchStateFactory.Copy(activeMatch.State);

                    return finishedState;
                }

                ValidateMatchVersion(activeMatch.State, expectedVersion);

                MatchTurn.ValidatePlayerTurn(activeMatch.State, player.PlayerId);

                CheckerColor playerColor = GetPlayerColor(
                    activeMatch.Session,
                    player.PlayerId);

                int nextPlayerId = GetOpponentPlayerId(activeMatch.Session, player.PlayerId);

                MatchTurnState previousTurn = activeMatch.State.Turn;

                MatchTurn.ConfirmTurn(activeMatch.State, playerColor, nextPlayerId);

                AdvanceTurnClock(activeMatch, previousTurn, nextPlayerId);

                activeMatch.TurnHistory.Clear();

                activeMatch.State.Version++;

                activeMatch.Clock.UpdateState();

                MatchState stateCopy = MatchStateFactory.Copy(activeMatch.State);

                return stateCopy;
            }
        }

        public static MatchState CompleteTimeout(ActiveMatch activeMatch)
        {
            if (activeMatch == null)
            {
                throw new ArgumentNullException(nameof(activeMatch));
            }

            lock (activeMatch.Queue)
            {
                ActiveMatch savedMatch = FindSavedActiveMatch(activeMatch.Session.RoomId);

                if (!ReferenceEquals(savedMatch, activeMatch))
                {
                    throw new KeyNotFoundException(MatchUnavailable);
                }

                CompleteExpiredMatch(activeMatch);

                activeMatch.Clock.UpdateState();

                MatchState stateCopy = MatchStateFactory.Copy(activeMatch.State);

                return stateCopy;
            }
        }

        internal static bool RemoveMatch(string roomId)
        {
            bool removedMatch = false;

            try
            {
                ActiveMatch activeMatch = ActiveMatches[roomId];

                lock (activeMatch.Queue)
                {
                    ActiveMatch savedMatch = ActiveMatches[roomId];

                    if (ReferenceEquals(savedMatch, activeMatch))
                    {
                        IDictionary<string, ActiveMatch> matches = ActiveMatches;

                        removedMatch = matches.Remove(roomId);
                    }
                }

                if (removedMatch)
                {
                    activeMatch.StopCountdown();
                    activeMatch.StopTurnTimer();
                }
            }
            catch (KeyNotFoundException matchException)
            {
                ApplicationLogger.LogWarning("The match was unavailable during removal. Reason: {0}",
                    matchException.Message);
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

        private static void RegisterReadyPlayer(ActiveMatch activeMatch, int playerId, IGameCallback playerCallback)
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

            ActiveMatch activeMatch = ActiveMatches[roomId];

            if (activeMatch == null)
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

        private static void ValidateMatchPlayer(ActiveMatch activeMatch, MatchPlayer player)
        {
            ActiveMatch savedMatch = FindSavedActiveMatch(player.RoomId);

            if (!ReferenceEquals(savedMatch, activeMatch) ||
                activeMatch.Session.MatchId != player.MatchId)
            {
                throw new KeyNotFoundException(MatchUnavailable);
            }

            bool isHost = activeMatch.Session.PlayerHostId == player.PlayerId;

            bool isGuest = activeMatch.Session.PlayerGuestId == player.PlayerId;

            if (!isHost && !isGuest)
            {
                throw new UnauthorizedAccessException(PlayerNotInMatch);
            }
        }

        private static void ValidateOpeningRollPhase(MatchPreparation preparation)
        {
            if (preparation.Phase != MatchPhase.OpeningRoll)
            {
                throw new ArgumentException(OpeningRollUnavailable);
            }
        }

        private static MatchOpeningRollState GetOpeningRoll(MatchOpeningRollState openingRollState, int roundNumber)
        {
            if (roundNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(roundNumber));
            }

            if (roundNumber == openingRollState.RoundNumber)
            {
                return openingRollState;
            }

            bool bothPlayersRolled = openingRollState.HostDieValue > 0 && openingRollState.GuestDieValue > 0;

            bool isTie = bothPlayersRolled && openingRollState.HostDieValue == openingRollState.GuestDieValue;

            bool isNextRound = roundNumber == openingRollState.RoundNumber + 1;

            if (!isTie || !isNextRound)
            {
                throw new ArgumentException(OpeningRollRoundUnavailable, nameof(roundNumber));
            }

            var nextOpeningRoll = new MatchOpeningRollState
            {
                RoundNumber = roundNumber
            };

            return nextOpeningRoll;
        }

        private static void RollPlayerDie(MatchSession matchSession, MatchOpeningRollState openingRollState,
            int playerId)
        {
            if (matchSession.PlayerHostId == playerId)
            {
                if (openingRollState.HostDieValue != 0)
                {
                    throw new ArgumentException(OpeningDieAlreadyRolled);
                }

                openingRollState.HostDieValue = Die.Roll();
                return;
            }

            if (openingRollState.GuestDieValue != 0)
            {
                throw new ArgumentException(OpeningDieAlreadyRolled);
            }

            openingRollState.GuestDieValue = Die.Roll();
        }

        private static void CompleteOpeningRoll(ActiveMatch activeMatch)
        {
            MatchPreparation preparation = activeMatch.Preparation;
            MatchOpeningRollState openingRoll = preparation.OpeningRoll;

            bool bothPlayersRolled = openingRoll.HostDieValue > NoDieValue && openingRoll.GuestDieValue > NoDieValue;

            if (!bothPlayersRolled || openingRoll.HostDieValue == openingRoll.GuestDieValue)
            {
                return;
            }

            int currentPlayerId = activeMatch.Session.PlayerGuestId;

            if (openingRoll.HostDieValue > openingRoll.GuestDieValue)
            {
                currentPlayerId = activeMatch.Session.PlayerHostId;
            }

            preparation.Phase = MatchPhase.WaitingForRoll;

            activeMatch.State.Turn.CurrentPlayerId = currentPlayerId;
            activeMatch.State.Phase = MatchPhase.WaitingForRoll;

            bool isHostTurn = currentPlayerId == activeMatch.Session.PlayerHostId;

            activeMatch.Clock.StartTurn(isHostTurn);

            activeMatch.State.Version++;
        }

        private static void ValidateMatchVersion(MatchState matchState, long expectedVersion)
        {
            if (matchState.Version != expectedVersion)
            {
                throw new ArgumentException("The match state is outdated.", nameof(expectedVersion));
            }
        }

        private static CheckerColor GetPlayerColor(MatchSession matchSession, int playerId)
        {
            CheckerColor playerColor = CheckerColor.None;

            if (matchSession.PlayerHostId == playerId)
            {
                playerColor = CheckerColor.Blue;
            }
            else if (matchSession.PlayerGuestId == playerId)
            {
                playerColor = CheckerColor.Pink;
            }
            else
            {
                throw new UnauthorizedAccessException(PlayerNotInMatch);
            }

            return playerColor;
        }

        private static int GetOpponentPlayerId(MatchSession matchSession, int playerId)
        {
            int opponentPlayerId;

            if (matchSession.PlayerHostId == playerId)
            {
                opponentPlayerId = matchSession.PlayerGuestId;
            }
            else if (matchSession.PlayerGuestId == playerId)
            {
                opponentPlayerId = matchSession.PlayerHostId;
            }
            else
            {
                throw new UnauthorizedAccessException(PlayerNotInMatch);
            }

            return opponentPlayerId;
        }

        private static bool CompleteExpiredMatch(ActiveMatch activeMatch)
        {
            bool finished = activeMatch.State.Phase == MatchPhase.Finished;

            if (finished)
            {
                return finished;
            }

            if (!activeMatch.Clock.HasExpired)
            {
                return finished;
            }

            int loserPlayerId = activeMatch.State.Turn.CurrentPlayerId;

            int winnerPlayerId = GetOpponentPlayerId(activeMatch.Session, loserPlayerId);

            bool completed = MatchCompletion.Complete(activeMatch, winnerPlayerId, MatchEndReason.TimeExpired);

            if (completed)
            {
                activeMatch.State.Version++;
            }

            finished = activeMatch.State.Phase == MatchPhase.Finished;

            return finished;
        }

        private static TimeSpan GetTurnTimerDelay(MatchClock clock)
        {
            TimeSpan remainingTime = clock.RemainingTurnTime;

            double milliseconds = Math.Ceiling(remainingTime.TotalMilliseconds);

            if (milliseconds < MinimumTurnTimerMilliseconds)
            {
                milliseconds = MinimumTurnTimerMilliseconds;
            }

            TimeSpan dueTime = TimeSpan.FromMilliseconds(milliseconds);

            return dueTime;
        }

        private static void AdvanceTurnClock(ActiveMatch activeMatch, MatchTurnState previousTurn, int nextPlayerId)
        {
            bool hasExpired = activeMatch.Clock.Stop();

            if (hasExpired)
            {
                activeMatch.State.Turn = previousTurn;

                MatchCompletion.Complete(activeMatch, nextPlayerId, MatchEndReason.TimeExpired);

                return;
            }

            bool isHostTurn = nextPlayerId == activeMatch.Session.PlayerHostId;

            activeMatch.Clock.StartTurn(isHostTurn);
        }
    }
}
