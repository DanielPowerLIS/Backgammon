

using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Services.Data;
using Backgammon.Server.Services.Manager;
using Backgammon.Server.SupportServices.Logging;
using System;
using System.ServiceModel;

namespace Backgammon.Server.Services
{
    internal class MatchService : IMatchService
    {
        private const string InvalidRequestCode = "InvalidRequest";
        private const string SessionExpiredCode = "SessionExpired";
        private const string AccessDeniedCode = "AccessDenied";
        private const string MatchUnavailableCode = "MatchUnavailable";
        private const string ServiceUnavailableCode = "ServiceUnavailable";

        private static readonly IApplicationLogger ApplicationLogger = new Log4NetApplicationLogger(typeof(MatchService));
        private readonly PlayerSessionManager _playerSessionManager = new PlayerSessionManager();

        public MatchSession InitializeMatch(string sessionId, string roomId)
        {
            ValidateMatchRequest(sessionId, roomId);

            try
            {
                PlayerSession playerSession = GetPlayerSessionForMatch(sessionId);
                MatchSession matchSession = CreateMatch(
                    playerSession.PlayerId, roomId, out IPreinitiationCallback guestCallback);

                NotifyGuest(guestCallback, matchSession);

                return matchSession;
            }
            catch (FaultException<MatchFault>)
            {
                throw;
            }
            catch (Exception exception)
            {
                ApplicationLogger.LogError(exception, "An unexpected error prevented match initialization.");
                throw CreateMatchFault(ServiceUnavailableCode);
            }
        }

        private void ValidateMatchRequest(string sessionId, string roomId)
        {
            if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(roomId))
            {
                ApplicationLogger.LogWarning(
                    "Match initialization was rejected because a required identifier is missing.");
                throw CreateMatchFault(InvalidRequestCode);
            }
        }

        private PlayerSession GetPlayerSessionForMatch(string sessionId)
        {
            try
            {
                PlayerSession playerSession = _playerSessionManager.GetSession(sessionId, true);

                return playerSession;
            }
            catch (UnauthorizedAccessException)
            {
                ApplicationLogger.LogWarning(
                    "Match initialization was rejected because the session is invalid or expired.");
                throw CreateMatchFault(SessionExpiredCode);
            }
        }

        private MatchSession CreateMatch(
            int playerId, string roomId, out IPreinitiationCallback guestCallback)
        {
            try
            {
                MatchSession matchSession = MatchManager.InitializeMatch(playerId, roomId, out guestCallback);

                return matchSession;
            }
            catch (UnauthorizedAccessException)
            {
                ApplicationLogger.LogWarning(
                    "Match initialization was rejected because the player is not the lobby host.");
                throw CreateMatchFault(AccessDeniedCode);
            }
            catch (InvalidOperationException)
            {
                ApplicationLogger.LogWarning(
                    "Match initialization was rejected because the match is unavailable.");
                throw CreateMatchFault(MatchUnavailableCode);
            }
        }

        private void NotifyGuest(IPreinitiationCallback guestCallback, MatchSession matchSession)
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

        public MatchSession JoinMatch(string sessionId, string roomId, string matchId)
        {
            try
            {
                PlayerSession playerSession = _playerSessionManager.GetSession(sessionId, true);
                MatchSession matchSession =
                    MatchManager.JoinMatch(playerSession.PlayerId, roomId, matchId);

                return matchSession;
            }
            catch (ArgumentException)
            {
                throw CreateMatchFault("InvalidRequest");
            }
            catch (UnauthorizedAccessException)
            {
                throw CreateMatchFault("AccessDenied");
            }
            catch (InvalidOperationException)
            {
                throw CreateMatchFault("MatchUnavailable");
            }
            catch (Exception exception)
            {
                ApplicationLogger.LogError(
                    exception,
                    "An unexpected error prevented the player from joining the match.");

                throw CreateMatchFault("ServiceUnavailable");
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

        public void MoveChecker(int fromPosition, int toPosition)
        {
            // Implementation for moving a checker
        }
    }
}
