using System;
using System.Collections.Generic;
using System.ServiceModel;

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

        public MatchState RegisterPlayerReady(string sessionId, string roomId, string matchId)
        {
            PlayerSession playerSession = GetPlayerSession(sessionId);

            try
            {
                IGameCallback playerCallback = OperationContext.Current.GetCallbackChannel<IGameCallback>();

                var request = new MatchPlayer(playerSession.PlayerId, roomId, matchId);

                MatchState matchState = MatchManager.RegisterPlayerReady(request, playerCallback);

                if (matchState.Phase == MatchPhase.StartingCountdown)
                {
                    MatchManager.StartCountdown(request, OnCountdownElapsed);
                }

                UpdatePlayers(request, matchState);

                return matchState;
            }
            catch (UnauthorizedAccessException accessException)
            {
                ApplicationLogger.LogWarning("The player was denied permission to register as ready. Reason: {0}",
                    accessException.Message);

                throw CreateMatchFault(ErrorCodes.AccessDenied);
            }
            catch (KeyNotFoundException matchException)
            {
                ApplicationLogger.LogWarning("The match is unavailable for player registration. Reason: {0}",
                    matchException.Message);

                throw CreateMatchFault(ErrorCodes.MatchUnavailable);
            }
            catch (ArgumentException requestException)
            {
                ApplicationLogger.LogWarning("The player readiness request was invalid. Reason: {0}",
                    requestException.Message);

                throw CreateMatchFault(ErrorCodes.InvalidRequest);
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

        private void OnCountdownElapsed(object timerState)
        {
            var activeMatch = (ActiveMatch)timerState;

            try
            {
                MatchState matchState = MatchManager.CompleteCountdown(activeMatch);

                var request = new MatchPlayer(
                    activeMatch.Session.PlayerHostId,
                    activeMatch.Session.RoomId,
                    activeMatch.Session.MatchId);

                UpdatePlayers(request, matchState);
            }
            catch (KeyNotFoundException matchException)
            {
                ApplicationLogger.LogDebug("The countdown ended after the match became unavailable. " +
                    "Reason: {0}",
                    matchException.Message);
            }
            catch (Exception countdownException)
            {
                ApplicationLogger.LogError(countdownException,
                    "An unexpected failure prevented the countdown " +
                    "from completing.");
            }
            finally
            {
                activeMatch.StopCountdown();
            }
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