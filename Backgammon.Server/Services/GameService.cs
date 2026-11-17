using System;
using System.Collections.Generic;
using System.ServiceModel;

using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Services.Data;
using Backgammon.Server.Services.Manager;
using Backgammon.Server.SupportServices.ExceptionCode;
using Backgammon.Server.SupportServices.Logging;

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