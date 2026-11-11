

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
        private static readonly IApplicationLogger ApplicationLogger = new Log4NetApplicationLogger(typeof(MatchService));
        private readonly PlayerSessionManager _playerSessionManager = new PlayerSessionManager();
        public MatchSession InitializeMatch(string sessionId, string roomId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    throw new ArgumentException("Session ID cannot be null or empty.", nameof(sessionId));
                }

                if (string.IsNullOrWhiteSpace(roomId))
                {
                    throw new ArgumentException("Room ID cannot be null or empty.", nameof(roomId));
                }

                PlayerSession playerSession = _playerSessionManager.GetSession(sessionId, true);
                int requestPlayerId = playerSession.PlayerId;
                MatchSession matchSession = MatchManager.InitializeMatch(
                    requestPlayerId, roomId, out IPreinitiationCallback guestCallback);

                NotifyGuest(guestCallback, matchSession);

                return matchSession;
            }
            catch (Exception exception)
            {
                // TODO: Quitar el log de error y manejar la excepción de manera más específica según el tipo de error.
                ApplicationLogger.LogError(exception,"The request to initialize a match failed.");

                throw;
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
