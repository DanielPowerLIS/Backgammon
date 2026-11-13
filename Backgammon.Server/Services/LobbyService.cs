using System;
using System.Collections.Generic;
using System.ServiceModel;

using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Services.Data;
using Backgammon.Server.Services.ExceptionCode;
using Backgammon.Server.Services.Manager;
using Backgammon.Server.SupportServices;
using Backgammon.Server.SupportServices.Logging;

namespace Backgammon.Server.Services
{
    public class LobbyService : ILobbyService
    {
        private readonly PlayerSessionManager _playerSessionManager =
            new PlayerSessionManager();

        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(LobbyService));

        public void LeaveLobby(string sessionId, string roomId)
        {
            PlayerSession playerSession = GetPlayerSession(sessionId);

            if (string.IsNullOrWhiteSpace(roomId))
            {
                ApplicationLogger.LogWarning(
                    "Lobby departure was rejected because the room code is missing.");

                throw CreateLobbyFault(ErrorCodes.LobbyLeaveFailed);
            }

            LeaveRoom(playerSession, roomId);

            ApplicationLogger.LogInformation("Player {0} left room {1}.", playerSession.PlayerId, roomId);
        }

        private PlayerSession GetPlayerSession(string sessionId)
        {
            try
            {
                PlayerSession playerSession =
                    _playerSessionManager.GetSession(sessionId, true);

                return playerSession;
            }
            catch (UnauthorizedAccessException sessionException)
            {
                ApplicationLogger.LogWarning(
                    "Lobby departure was rejected because the session is invalid or expired. Reason: {0}",
                    sessionException.Message);

                throw CreateLobbyFault(ErrorCodes.SessionExpired);
            }
        }

        private void LeaveRoom(PlayerSession playerSession, string roomId)
        {
            try
            {
                bool isGuest = LobbyManager.IsGuestInRoom(playerSession.PlayerId, roomId);

                if (isGuest)
                {
                    LobbySession lobbySession = LobbyManager.LeaveRoomAsGuest(playerSession, roomId);

                    UpdateHost(lobbySession);
                }
                else
                {
                    LobbySession closedLobbySession = LobbyManager.LeaveRoomAsHost(playerSession, roomId);

                    CloseGuestLobby(closedLobbySession);
                }
            }
            catch (UnauthorizedAccessException accessException)
            {
                ApplicationLogger.LogWarning(
                    "Player {0} cannot leave room {1}. Reason: {2}",
                    playerSession.PlayerId, roomId, accessException.Message);

                throw CreateLobbyFault(ErrorCodes.AccessDenied);
            }
            catch (KeyNotFoundException lobbyException)
            {
                ApplicationLogger.LogWarning(
                    "Departure from room {0} could not be completed. Reason: {1}",
                    roomId, lobbyException.Message);

                throw CreateLobbyFault(ErrorCodes.LobbyLeaveFailed);
            }
        }

        private void UpdateHost(LobbySession lobbySession)
        {
            Lobby lobby = lobbySession.RoomData;

            try
            {
                lobbySession.HostCallback.OnLobbyUpdated(lobby);
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(
                    communicationException,
                    "A communication failure prevented the host from receiving the departure update.");
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(
                    timeoutException,
                    "The departure callback to the host exceeded the time limit.");
            }
            catch (ObjectDisposedException disposedException)
            {
                ApplicationLogger.LogError(
                    disposedException,
                    "The host channel was disposed before the departure update could be sent.");
            }
        }

        private void CloseGuestLobby(LobbySession lobbySession)
        {
            if (lobbySession.GuestCallback == null)
            {
                return;
            }

            try
            {
                lobbySession.GuestCallback.OnCloseLobby(false);
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(
                    communicationException,
                    "A communication failure prevented the guest from receiving the lobby closure.");
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(
                    timeoutException,
                    "The closure callback to the guest exceeded the time limit.");
            }
            catch (ObjectDisposedException disposedException)
            {
                ApplicationLogger.LogError(
                    disposedException,
                    "The guest channel was disposed before the lobby closure could be sent.");
            }
        }

        private FaultException<LobbyFault> CreateLobbyFault(string errorCode)
        {
            var lobbyFault = new LobbyFault
            {
                ErrorCode = errorCode
            };

            var faultException = new FaultException<LobbyFault>(lobbyFault,new FaultReason(
                    "The lobby operation could not be completed."));

            return faultException;
        }
    }
}
