using System;
using System.Collections.Generic;
using System.ServiceModel;

using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Services.Data;
using Backgammon.Server.Services.ExceptionCode;
using Backgammon.Server.Services.Manager;
using Backgammon.Server.SupportServices.Logging;

namespace Backgammon.Server.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class ChatService : IChatService
    {
        private const string ChatOperationErrorMessage = "The chat operation could not be completed.";

        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(ChatService));

        private readonly PlayerSessionManager _playerSessionManager = new PlayerSessionManager();

        public List<ChatMessage> JoinLobbyChat(string sessionId, string roomId)
        {
            try
            {
                PlayerSession playerSession = GetChatPlayerSession(sessionId);
                IChatCallback chatCallback =
                    OperationContext.Current.GetCallbackChannel<IChatCallback>();
                List<ChatMessage> chatMessages =
                    LobbyManager.JoinChat(playerSession, roomId, chatCallback);

                return chatMessages;
            }
            catch (ArgumentException validationException)
            {
                ApplicationLogger.LogWarning(
                    "The request to join the lobby chat was invalid. Reason: {0}",
                    validationException.Message);

                throw CreateChatFault(ErrorCodes.InvalidRequest);
            }
            catch (UnauthorizedAccessException accessException)
            {
                ApplicationLogger.LogWarning(
                    "Access to the lobby chat was denied. Reason: {0}",
                    accessException.Message);

                throw CreateChatFault(ErrorCodes.AccessDenied);
            }
            catch (KeyNotFoundException lobbyException)
            {
                ApplicationLogger.LogWarning(
                    "The lobby was not found when joining the chat. Reason: {0}",
                    lobbyException.Message);

                throw CreateChatFault(ErrorCodes.LobbyUnavailable);
            }
        }

        public void SendMessage(string sessionId, string roomId, string message)
        {
            try
            {
                PlayerSession playerSession = GetChatPlayerSession(sessionId);
                LobbyManager.SendMessage(playerSession, roomId, message);
            }
            catch (ArgumentException validationException)
            {
                ApplicationLogger.LogWarning(
                    "The request to send a chat message was invalid. Reason: {0}",
                    validationException.Message);

                throw CreateChatFault(ErrorCodes.InvalidRequest);
            }
            catch (UnauthorizedAccessException accessException)
            {
                ApplicationLogger.LogWarning(
                    "The player was denied permission to send a chat message. Reason: {0}",
                    accessException.Message);

                throw CreateChatFault(ErrorCodes.AccessDenied);
            }
            catch (KeyNotFoundException lobbyException)
            {
                ApplicationLogger.LogWarning(
                    "The lobby was not found when sending a chat message. Reason: {0}",
                    lobbyException.Message);

                throw CreateChatFault(ErrorCodes.LobbyUnavailable);
            }
        }

        public void LeaveLobbyChat(string sessionId, string roomId)
        {
            try
            {
                PlayerSession playerSession = GetChatPlayerSession(sessionId);
                LobbyManager.LeaveChat(playerSession, roomId);
            }
            catch (ArgumentException validationException)
            {
                ApplicationLogger.LogWarning(
                    "The request to leave the lobby chat was invalid. Reason: {0}",
                    validationException.Message);

                throw CreateChatFault(ErrorCodes.InvalidRequest);
            }
        }

        private PlayerSession GetChatPlayerSession(string sessionId)
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
                    "The chat request was rejected because the session is invalid. Reason: {0}",
                    sessionException.Message);

                throw CreateChatFault(ErrorCodes.SessionExpired);
            }
        }

        private FaultException<ChatFault> CreateChatFault(string errorCode)
        {
            var chatFault = new ChatFault
            {
                ErrorCode = errorCode
            };

            var faultException = new FaultException<ChatFault>(chatFault, ChatOperationErrorMessage);

            return faultException;
        }
    }
}
