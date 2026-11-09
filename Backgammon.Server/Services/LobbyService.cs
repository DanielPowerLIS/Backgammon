using System;
using System.ServiceModel;

using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Services.Data;
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

        private static readonly string SessionExpiredErrorCode = "SessionExpired";

        private static readonly string LeaveLobbyErrorCode = "LobbyLeaveFailed";

        private static readonly string AccessDeniedErrorCode = "AccessDenied";

        public void LeaveLobby(string sessionId, string roomId)
        {
            PlayerSession playerSession = GetPlayerSession(sessionId);

            if (string.IsNullOrWhiteSpace(roomId))
            {
                ApplicationLogger.LogWarning(
                    "Lobby departure was rejected because the room code is missing.");

                throw CreateLobbyFault(LeaveLobbyErrorCode);
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

                throw CreateLobbyFault(SessionExpiredErrorCode);
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

                throw CreateLobbyFault(AccessDeniedErrorCode);
            }
            catch (InvalidOperationException lobbyException)
            {
                ApplicationLogger.LogWarning(
                    "Departure from room {0} could not be completed. Reason: {1}",
                    roomId, lobbyException.Message);

                throw CreateLobbyFault(LeaveLobbyErrorCode);
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
                    "A communication failure prevented the host of room {0} from receiving the departure update.",
                    lobby.RoomId);
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(
                    timeoutException,
                    "The departure callback to the host of room {0} exceeded the time limit.",
                    lobby.RoomId);
            }
            catch (ObjectDisposedException disposedException)
            {
                ApplicationLogger.LogError(
                    disposedException,
                    "The departure callback to the host of room {0} could not " +
                    "be completed because the channel was disposed.",
                    lobby.RoomId);
            }
        }

        private void CloseGuestLobby(LobbySession lobbySession)
        {
            if (lobbySession.GuestCallback == null)
            {
                return;
            }

            string roomId = lobbySession.RoomData.RoomId;

            try
            {
                lobbySession.GuestCallback.OnCloseLobby(false);
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(communicationException,
                    "A communication failure prevented the guest of room {0} " +
                    "from receiving the lobby closure.",
                    roomId);
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(timeoutException,
                    "The closure callback to the guest of room {0} " +
                    "exceeded the time limit.",
                    roomId);
            }
            catch (ObjectDisposedException disposedException)
            {
                ApplicationLogger.LogError(disposedException,
                    "The closure callback to the guest of room {0} " +
                    "could not be completed because the channel was disposed.",
                    roomId);
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
