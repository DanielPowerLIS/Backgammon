using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Security.Cryptography;

using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Services.Data;
using Backgammon.Server.Services.Manager;
using Backgammon.Server.SupportServices.ExceptionCode;
using Backgammon.Server.SupportServices.Logging;
using Backgammon.Server.Domain;

namespace Backgammon.Server.Services
{
    internal class GameService : IGameService
    {
        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(GameService));

        private readonly PlayerSessionManager _playerSessionManager;

        public GameService()
        {
            _playerSessionManager = new PlayerSessionManager();
        }

        public MatchState GetMatchState(string sessionId, string roomId, string matchId)
        {
            PlayerSession playerSession = GetPlayerSession(sessionId);

            try
            {
                MatchState matchState = MatchManager.GetMatchState(playerSession.PlayerId, roomId, matchId);

                return matchState;
            }
            catch (UnauthorizedAccessException accessException)
            {
                ApplicationLogger.LogWarning("The player was denied access to the match state. Reason: {0}",
                    accessException.Message);

                throw CreateMatchFault(ErrorCodes.AccessDenied);
            }
            catch (KeyNotFoundException matchException)
            {
                ApplicationLogger.LogWarning("The requested match state is unavailable. Reason: {0}",
                    matchException.Message);

                throw CreateMatchFault(ErrorCodes.MatchUnavailable);
            }
            catch (ArgumentException requestException)
            {
                ApplicationLogger.LogWarning("The match state request was invalid. Reason: {0}",
                    requestException.Message);

                throw CreateMatchFault(ErrorCodes.InvalidRequest);
            }
        }

        public MatchPreparation GetMatchPreparation( string sessionId, string roomId, string matchId)
        {
            PlayerSession playerSession = GetPlayerSession(sessionId);

            try
            {
                var player = new MatchPlayer(playerSession.PlayerId, roomId, matchId);

                MatchPreparation preparation = MatchManager.GetMatchPreparation(player);

                return preparation;
            }
            catch (UnauthorizedAccessException accessException)
            {
                ApplicationLogger.LogWarning("Access to the match preparation was denied. Reason: {0}",
                    accessException.Message);

                throw CreateMatchFault(ErrorCodes.AccessDenied);
            }
            catch (KeyNotFoundException matchException)
            {
                ApplicationLogger.LogWarning("The match preparation is unavailable. Reason: {0}",
                    matchException.Message);

                throw CreateMatchFault(ErrorCodes.MatchUnavailable);
            }
            catch (ArgumentException preparationException)
            {
                ApplicationLogger.LogWarning("The match preparation query was invalid. Reason: {0}",
                    preparationException.Message);

                throw CreateMatchFault(ErrorCodes.InvalidRequest);
            }
        }

        public MatchPreparation RegisterPlayerReady(string sessionId, string roomId, string matchId)
        {
            PlayerSession playerSession = GetPlayerSession(sessionId);

            try
            {
                MatchPreparation preparation = RegisterReadyPlayer(playerSession, roomId, matchId);

                return preparation;
            }
            catch (UnauthorizedAccessException accessException)
            {
                ApplicationLogger.LogWarning("The player readiness registration was denied. Reason: {0}",
                    accessException.Message);

                throw CreateMatchFault(ErrorCodes.AccessDenied);
            }
            catch (KeyNotFoundException matchException)
            {
                ApplicationLogger.LogWarning("The match is unavailable for player registration. Reason: {0}",
                    matchException.Message);

                throw CreateMatchFault(ErrorCodes.MatchUnavailable);
            }
            catch (ArgumentException preparationException)
            {
                ApplicationLogger.LogWarning("The player readiness registration was invalid. Reason: {0}",
                    preparationException.Message);

                throw CreateMatchFault(ErrorCodes.InvalidRequest);
            }
        }

        public MatchPreparation RollOpeningDie(string sessionId, PlayerOpeningRoll playerRoll)
        {
            PlayerSession playerSession = GetPlayerSession(sessionId);

            try
            {
                MatchPreparation preparation = RollPlayerOpeningDie(playerSession, playerRoll);

                return preparation;
            }
            catch (UnauthorizedAccessException accessException)
            {
                ApplicationLogger.LogWarning("The opening roll was denied. Reason: {0}", accessException.Message);

                throw CreateMatchFault(ErrorCodes.AccessDenied);
            }
            catch (KeyNotFoundException matchException)
            {
                ApplicationLogger.LogWarning("The opening roll match is unavailable. Reason: {0}",
                    matchException.Message);

                throw CreateMatchFault(ErrorCodes.MatchUnavailable);
            }
            catch (ArgumentException rollException)
            {
                ApplicationLogger.LogWarning("The opening roll was invalid. Reason: {0}", rollException.Message);

                throw CreateMatchFault(ErrorCodes.InvalidInput);
            }
            catch (CryptographicException generationException)
            {
                ApplicationLogger.LogError(generationException, "The opening die could not be generated.");

                throw CreateMatchFault(ErrorCodes.ServiceUnavailable);
            }
        }

        public MatchState RollDice(string sessionId, PlayerTurn playerRoll)
        {
            PlayerSession playerSession = GetPlayerSession(sessionId);

            try
            {
                MatchState matchState = RollPlayerDice(playerSession, playerRoll);

                return matchState;
            }
            catch (UnauthorizedAccessException accessException)
            {
                ApplicationLogger.LogWarning("The turn roll was denied. Reason: {0}", accessException.Message);

                throw CreateMatchFault(ErrorCodes.AccessDenied);
            }
            catch (KeyNotFoundException matchException)
            {
                ApplicationLogger.LogWarning("The turn roll match is unavailable. Reason: {0}", matchException.Message);

                throw CreateMatchFault(ErrorCodes.MatchUnavailable);
            }
            catch (ArgumentException rollException)
            {
                ApplicationLogger.LogWarning("The turn roll was invalid. Reason: {0}", rollException.Message);

                throw CreateMatchFault(ErrorCodes.InvalidInput);
            }
            catch (CryptographicException generationException)
            {
                ApplicationLogger.LogError(generationException, "The turn dice could not be generated.");

                throw CreateMatchFault(ErrorCodes.ServiceUnavailable);
            }
        }

        public MatchState MoveChecker(string sessionId, PlayerCheckerMove playerMove)
        {
            PlayerSession playerSession = GetPlayerSession(sessionId);

            try
            {
                MatchState matchState = MovePlayerChecker(playerSession, playerMove);

                return matchState;
            }
            catch (UnauthorizedAccessException accessException)
            {
                ApplicationLogger.LogWarning("The checker movement was denied. Reason: {0}", accessException.Message);

                throw CreateMatchFault(ErrorCodes.AccessDenied);
            }
            catch (KeyNotFoundException matchException)
            {
                ApplicationLogger.LogWarning("The checker movement match is unavailable. Reason: {0}",
                    matchException.Message);

                throw CreateMatchFault(ErrorCodes.MatchUnavailable);
            }
            catch (ArgumentException moveException)
            {
                ApplicationLogger.LogWarning("The checker movement was invalid. Reason: {0}", moveException.Message);

                throw CreateMatchFault(ErrorCodes.InvalidInput);
            }
        }

        public MatchState UndoCheckerMove(string sessionId, PlayerTurn playerTurn)
        {
            PlayerSession playerSession = GetPlayerSession(sessionId);

            try
            {
                MatchState matchState = UndoPlayerCheckerMove(playerSession, playerTurn);

                return matchState;
            }
            catch (UnauthorizedAccessException accessException)
            {
                ApplicationLogger.LogWarning("The checker movement undo was denied. Reason: {0}",
                    accessException.Message);

                throw CreateMatchFault(ErrorCodes.AccessDenied);
            }
            catch (KeyNotFoundException matchException)
            {
                ApplicationLogger.LogWarning("The match is unavailable for movement undo. Reason: {0}",
                    matchException.Message);

                throw CreateMatchFault(ErrorCodes.MatchUnavailable);
            }
            catch (ArgumentException undoException)
            {
                ApplicationLogger.LogWarning("The checker movement undo was invalid. Reason: {0}",
                    undoException.Message);

                throw CreateMatchFault(ErrorCodes.InvalidInput);
            }
        }

        public MatchState ConfirmTurn(string sessionId, PlayerTurn playerTurn)
        {
            PlayerSession playerSession = GetPlayerSession(sessionId);

            try
            {
                MatchState matchState = ConfirmPlayerTurn(playerSession, playerTurn);

                return matchState;
            }
            catch (UnauthorizedAccessException accessException)
            {
                ApplicationLogger.LogWarning("The turn confirmation was denied. Reason: {0}",
                    accessException.Message);

                throw CreateMatchFault(ErrorCodes.AccessDenied);
            }
            catch (KeyNotFoundException matchException)
            {
                ApplicationLogger.LogWarning("The match is unavailable for turn confirmation. Reason: {0}",
                    matchException.Message);

                throw CreateMatchFault(ErrorCodes.MatchUnavailable);
            }
            catch (ArgumentException confirmationException)
            {
                ApplicationLogger.LogWarning("The turn confirmation was invalid. Reason: {0}",
                    confirmationException.Message);

                throw CreateMatchFault(ErrorCodes.InvalidInput);
            }
        }

        private PlayerSession GetPlayerSession(string sessionId)
        {
            try
            {
                PlayerSession playerSession = _playerSessionManager.GetSession(sessionId, true);

                return playerSession;
            }
            catch (UnauthorizedAccessException sessionException)
            {
                ApplicationLogger.LogWarning("The game operation was rejected because " +
                    "the session is invalid or expired. Reason: {0}",
                    sessionException.Message);

                throw CreateMatchFault(ErrorCodes.SessionExpired);
            }
        }

        private void UpdatePlayers(MatchPlayer request, MatchState matchState)
        {
            IGameCallback[] callbacks = MatchManager.GetGameCallbacks(request);

            foreach (IGameCallback playerCallback in callbacks)
            {
                UpdatePlayer(playerCallback, matchState);
            }
        }

        private void UpdatePlayers( MatchPlayer player, MatchPreparation preparation)
        {
            IGameCallback[] callbacks = MatchManager.GetGameCallbacks(player);

            foreach (IGameCallback playerCallback in callbacks)
            {
                UpdatePlayer(playerCallback, preparation);
            }
        }

        private void UpdatePlayer(IGameCallback playerCallback, MatchState matchState)
        {
            try
            {
                playerCallback.OnMatchStateUpdated(matchState);
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(communicationException,
                    "The match state could not be sent to a player.");
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(timeoutException,
                    "Sending the match state exceeded the time limit.");
            }
            catch (ObjectDisposedException disposedException)
            {
                ApplicationLogger.LogError(disposedException,
                    "The player's game callback is already disposed.");
            }
        }

        private void UpdatePlayer(IGameCallback playerCallback, MatchPreparation preparation)
        {
            try
            {
                playerCallback.OnMatchPreparationUpdated(preparation);
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(communicationException,
                    "The match preparation could not be sent to a player.");
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(timeoutException,
                    "Sending the match preparation exceeded the time limit.");
            }
            catch (ObjectDisposedException disposedException)
            {
                ApplicationLogger.LogError(disposedException,
                    "The player's preparation callback is already disposed.");
            }
        }

        private MatchPreparation RegisterReadyPlayer(PlayerSession playerSession, string roomId, string matchId)
        {
            IGameCallback playerCallback = OperationContext.Current.GetCallbackChannel<IGameCallback>();

            var player = new MatchPlayer(playerSession.PlayerId, roomId, matchId);

            MatchPreparation preparation = MatchManager.RegisterPlayerReady(player, playerCallback);

            if (preparation.Phase == MatchPhase.StartingCountdown)
            {
                MatchManager.StartCountdown(player, OnCountdownElapsed);
            }

            UpdatePlayers(player, preparation);

            return preparation;
        }

        private void OnCountdownElapsed(object timerState)
        {
            var activeMatch = (ActiveMatch)timerState;

            try
            {
                MatchPreparation preparation = MatchManager.CompleteCountdown(activeMatch);

                var player = new MatchPlayer(
                    activeMatch.Session.PlayerHostId,
                    activeMatch.Session.RoomId,
                    activeMatch.Session.MatchId);

                UpdatePlayers(player, preparation);
            }
            catch (KeyNotFoundException matchException)
            {
                ApplicationLogger.LogDebug("The countdown match is no longer available. Reason: {0}",
                    matchException.Message);
            }
            catch (Exception countdownException)
            {
                ApplicationLogger.LogError(countdownException,
                    "An unexpected failure prevented the countdown from completing.");
            }
            finally
            {
                activeMatch.StopCountdown();
            }
        }

        private MatchPreparation RollPlayerOpeningDie(PlayerSession playerSession, PlayerOpeningRoll playerRoll)
        {
            if (playerRoll == null)
            {
                throw new ArgumentNullException(nameof(playerRoll));
            }

            var player = new MatchPlayer(playerSession.PlayerId, playerRoll.RoomId, playerRoll.MatchId);

            MatchPreparation preparation = MatchManager.RollOpeningDie(player, playerRoll.RoundNumber);

            UpdatePlayers(player, preparation);

            if (preparation.Phase == MatchPhase.WaitingForRoll)
            {
                MatchState matchState = MatchManager.GetMatchState(
                    playerSession.PlayerId,
                    player.RoomId,
                    player.MatchId);

                UpdatePlayers(player, matchState);
            }

            return preparation;
        }

        private MatchState RollPlayerDice(PlayerSession playerSession, PlayerTurn playerRoll)
        {
            if (playerRoll == null)
            {
                throw new ArgumentNullException(nameof(playerRoll));
            }

            var player = new MatchPlayer(
                playerSession.PlayerId,
                playerRoll.RoomId,
                playerRoll.MatchId);

            MatchState matchState = MatchManager.RollDice(player, playerRoll.ExpectedVersion);

            UpdatePlayers(player, matchState);

            return matchState;
        }

        private MatchState MovePlayerChecker(PlayerSession playerSession, PlayerCheckerMove playerMove)
        {
            if (playerMove == null)
            {
                throw new ArgumentNullException(nameof(playerMove));
            }

            var player = new MatchPlayer(playerSession.PlayerId, playerMove.RoomId, playerMove.MatchId);

            MatchState matchState = MatchManager.MoveChecker(player, playerMove.Move, playerMove.ExpectedVersion);

            UpdatePlayers(player, matchState);

            return matchState;
        }

        private MatchState UndoPlayerCheckerMove(PlayerSession playerSession, PlayerTurn playerTurn)
        {
            if (playerTurn == null)
            {
                throw new ArgumentNullException(nameof(playerTurn));
            }

            var player = new MatchPlayer(
                playerSession.PlayerId,
                playerTurn.RoomId,
                playerTurn.MatchId);

            MatchState matchState = MatchManager.UndoCheckerMove(player, playerTurn.ExpectedVersion);

            UpdatePlayers(player, matchState);

            return matchState;
        }

        private MatchState ConfirmPlayerTurn(PlayerSession playerSession, PlayerTurn playerTurn)
        {
            if (playerTurn == null)
            {
                throw new ArgumentNullException(nameof(playerTurn));
            }

            var player = new MatchPlayer(
                playerSession.PlayerId,
                playerTurn.RoomId,
                playerTurn.MatchId);

            MatchState matchState = MatchManager.ConfirmTurn(player, playerTurn.ExpectedVersion);

            UpdatePlayers(player, matchState);

            return matchState;
        }

        private FaultException<MatchFault> CreateMatchFault(string errorCode)
        {
            var matchFault = new MatchFault
            {
                ErrorCode = errorCode
            };

            var faultException = new FaultException<MatchFault>(matchFault,
                "The game operation could not be completed.");

            return faultException;
        }
    }
}