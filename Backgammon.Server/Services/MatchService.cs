using System;
using System.Collections.Generic;
using System.ServiceModel;

using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Services.Data;
using Backgammon.Server.SupportServices.ExceptionCode;
using Backgammon.Server.Services.Manager;
using Backgammon.Server.SupportServices.Logging;

namespace Backgammon.Server.Services
{
    internal class MatchService : IMatchService
    {
        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(MatchService));

        private readonly PlayerSessionManager _playerSessionManager = new PlayerSessionManager();

        public MatchSession StartMatch(string sessionId, string roomId)
        {
            ValidateMatchRequest(sessionId, roomId);

            PlayerSession playerSession = GetPlayerSession(sessionId);

            MatchSession matchSession = CreateMatch(playerSession.PlayerId, roomId);

            IPreinitiationCallback guestCallback = GetGuestCallback(matchSession);

            UpdateGuest(guestCallback, matchSession);

            return matchSession;
        }

        public MatchSession JoinMatch(string sessionId, string roomId, string matchId)
        {
            try
            {
                PlayerSession playerSession = _playerSessionManager.GetSession(sessionId, true);
                MatchSession matchSession = MatchManager.JoinMatch(playerSession.PlayerId, roomId, matchId);

                return matchSession;
            }
            catch (UnauthorizedAccessException accessException)
            {
                ApplicationLogger.LogWarning(
                    "The player was denied access to the requested match. Reason: {0}",
                    accessException.Message);

                throw CreateMatchFault(ErrorCodes.AccessDenied);
            }
            catch (KeyNotFoundException matchException)
            {
                ApplicationLogger.LogWarning(
                    "The requested match is unavailable. Reason: {0}",
                    matchException.Message);

                throw CreateMatchFault(ErrorCodes.MatchUnavailable);
            }
            catch (ArgumentException requestException)
            {
                ApplicationLogger.LogWarning(
                    "The match join request was invalid. Reason: {0}",
                    requestException.Message);

                throw CreateMatchFault(ErrorCodes.InvalidRequest);
            }
        }

        private void ValidateMatchRequest(string sessionId, string roomId)
        {
            if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(roomId))
            {
                ApplicationLogger.LogWarning(
                    "Match initialization was rejected because a required identifier is missing.");

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
                ApplicationLogger.LogWarning(
                    "Match initialization was rejected because the session is invalid. Reason: {0}",
                    sessionException.Message);

                throw CreateMatchFault(ErrorCodes.SessionExpired);
            }
        }

        private IPreinitiationCallback GetGuestCallback(MatchSession matchSession)
        {
            try
            {
                IPreinitiationCallback guestCallback = MatchManager.GetGuestCallback(matchSession);

                return guestCallback;
            }
            catch (KeyNotFoundException matchException)
            {
                ApplicationLogger.LogWarning(
                    "The guest callback for room {0} could not be retrieved. Reason: {1}",
                    matchSession.RoomId, matchException.Message);

                throw CreateMatchFault(ErrorCodes.MatchUnavailable);
            }
        }

        private MatchSession CreateMatch(int playerId, string roomId)
        {
            try
            {
                MatchSession matchSession = MatchManager.CreateMatch(playerId, roomId);

                return matchSession;
            }
            catch (UnauthorizedAccessException accessException)
            {
                ApplicationLogger.LogWarning(
                    "Match initialization was rejected because the player is not the lobby host. " +
                    "Reason: {0}",
                    accessException.Message);

                throw CreateMatchFault(ErrorCodes.AccessDenied);
            }
            catch (KeyNotFoundException matchException)
            {
                ApplicationLogger.LogWarning(
                    "Match initialization was rejected because the lobby or guest is unavailable. " +
                    "Reason: {0}",
                    matchException.Message);

                throw CreateMatchFault(ErrorCodes.MatchUnavailable);
            }
            catch (ArgumentException matchException)
            {
                ApplicationLogger.LogWarning(
                    "Match initialization was rejected because a match is already active. Reason: {0}",
                    matchException.Message);

                throw CreateMatchFault(ErrorCodes.MatchUnavailable);
            }
        }

        private void UpdateGuest(IPreinitiationCallback guestCallback, MatchSession matchSession)
        {
            if (guestCallback == null)
            {
                return;
            }

            try
            {
                guestCallback.OnMatchStarted(matchSession.RoomId, matchSession.MatchId);
            }
            catch (CommunicationException exception)
            {
                ApplicationLogger.LogError(exception, "The guest could not receive the match notification.");
            }
            catch (TimeoutException exception)
            {
                ApplicationLogger.LogError(exception, "The match notification timed out.");
            }
            catch (ObjectDisposedException exception)
            {
                ApplicationLogger.LogError(exception, "The guest callback was closed.");
            }
        }

        private FaultException<MatchFault> CreateMatchFault(string errorCode)
        {
            var matchFault = new MatchFault
            {
                ErrorCode = errorCode
            };

            var faultException = new FaultException<MatchFault>(
                matchFault,
                "The match operation could not be completed.");

            return faultException;
        }
    }
}
