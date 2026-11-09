using System;
using System.ServiceModel;

using System.Text.RegularExpressions;

using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Services.Data;
using Backgammon.Server.Services.Manager;
using Backgammon.Server.SupportServices;
using Backgammon.Server.SupportServices.Logging;

namespace Backgammon.Server.Services
{

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class PreinitiationService : IPreinitiationService
    {
        private readonly PlayerSessionManager _playerSessionManager = new PlayerSessionManager();

        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(PreinitiationService));

        private static readonly string JoinLobbyErrorCode = "LobbyJoinFailed";
        private static readonly string SessionExpiredErrorCode = "SessionExpired";
        private static readonly string InvalidRoomCodeErrorCode = "InvalidRoomCode";
        private static readonly string ServiceUnavailableErrorCode = "ServiceUnavailable";

        private static readonly string RoomCodePattern = @"\A[A-Z0-9]{6}\z";

        private static readonly TimeSpan RoomCodeValidationTimeout = TimeSpan.FromSeconds(1);

        private static readonly Regex RoomCodeExpression = new Regex(RoomCodePattern, RegexOptions.None,
            RoomCodeValidationTimeout);

        public Lobby CreateLobby(string sessionId)
        {

            PlayerSession playerSession = GetPlayerSession(sessionId);

            IPreinitiationCallback callback = OperationContext.Current.GetCallbackChannel<IPreinitiationCallback>();

            Lobby lobby = LobbyManager.CreateRoom(playerSession, callback);

            return lobby;
        }

        public Lobby JoinLobby(string sessionId, string roomId)
        {
            PlayerSession playerSession = GetPlayerSession(sessionId);
            ValidateRoomCode(roomId);
            IPreinitiationCallback guestCallback = 
                OperationContext.Current.GetCallbackChannel<IPreinitiationCallback>();

            try
            {
                LobbySession lobbySession = LobbyManager.JoinRoom(playerSession, roomId, guestCallback);
                Lobby lobby = lobbySession.RoomData;
                UpdateHost(lobbySession, lobby);

                return lobby;
            }
            catch (InvalidOperationException lobbyException)
            {
                ApplicationLogger.LogWarning("The player could not join the lobby. Reason: {0}",
                    lobbyException.Message);

                throw CreatePreinitiationFault(JoinLobbyErrorCode);
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
                    "The lobby operation was rejected because the session is invalid or expired. Reason: {0}",
                    sessionException.Message);

                throw CreatePreinitiationFault(SessionExpiredErrorCode);
            }
        }

        private void ValidateRoomCode(string roomId)
        {
            try
            {
                bool validateRoomCode = roomId != null && RoomCodeExpression.IsMatch(roomId);

                if (!validateRoomCode)
                {
                    throw CreatePreinitiationFault(InvalidRoomCodeErrorCode);
                }
            }
            catch (RegexMatchTimeoutException validationException)
            {
                ApplicationLogger.LogError(validationException,
                    "Room code validation exceeded the execution time limit.");

                throw CreatePreinitiationFault(ServiceUnavailableErrorCode);
            }
        }

        private void UpdateHost(LobbySession lobbySession, Lobby lobby)
        {
            int guestPlayerId = lobby.GuestPlayerId;

            try
            {
                lobbySession.HostCallback.OnLobbyUpdated(lobby);
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(communicationException,
                    "A communication failure prevented the host of room {0} from being notified.",
                    lobby.RoomId);

                LobbyManager.CancelJoin(lobbySession, guestPlayerId);

                throw CreatePreinitiationFault(ServiceUnavailableErrorCode);
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(timeoutException,
                    "The callback to the host of room {0} exceeded the time limit.",
                    lobby.RoomId);

                LobbyManager.CancelJoin(lobbySession, guestPlayerId);

                throw CreatePreinitiationFault(ServiceUnavailableErrorCode);
            }
            catch (ObjectDisposedException disposedException)
            {
                ApplicationLogger.LogError(disposedException,
                    "The callback to the host of room {0} could not be completed because the channel was already disposed.",
                    lobby.RoomId);

                LobbyManager.CancelJoin(lobbySession, guestPlayerId);

                throw CreatePreinitiationFault(ServiceUnavailableErrorCode);
            }
        }

        private FaultException<PreinitiationFault> CreatePreinitiationFault(
            string errorCode)
        {
            var fault = new PreinitiationFault
            {
                ErrorCode = errorCode
            };

            var faultException = new FaultException<PreinitiationFault>(
                fault,
                new FaultReason("The lobby operation could not be completed."));

            return faultException;
        }
    }
}
