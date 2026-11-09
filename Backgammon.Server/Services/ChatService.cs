using System;
using System.Collections.Generic;
using System.ServiceModel;

using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Services.Data;
using Backgammon.Server.Services.Manager;
using Backgammon.Server.SupportServices.Logging;

namespace Backgammon.Server.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class ChatService : IChatService
    {
        private readonly PlayerSessionManager _playerSessionManager = new PlayerSessionManager();

        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(ChatService));

        private const string InvalidRequestCode = "InvalidRequest";
        private const string AccessDeniedCode = "AccessDenied";
        private const string LobbyUnavailableCode = "LobbyUnavailable";
        private const string ServiceUnavailableCode = "ServiceUnavailable";
        private const string ChatOperationErrorMessage = "The chat operation could not be completed.";

        public List<ChatMessage> JoinLobbyChat(string sessionId, string roomId)
        {
            try
            {
                PlayerSession playerSession = _playerSessionManager.GetSession(sessionId, true);
                IChatCallback chatCallback =
                    OperationContext.Current.GetCallbackChannel<IChatCallback>();
                List<ChatMessage> chatMessages =
                    LobbyManager.JoinChat(playerSession, roomId, chatCallback);

                return chatMessages;
            }
            catch (ArgumentException)
            {
                throw CreateExpectedChatFault(InvalidRequestCode, nameof(JoinLobbyChat));
            }
            catch (UnauthorizedAccessException)
            {
                throw CreateExpectedChatFault(AccessDeniedCode, nameof(JoinLobbyChat));
            }
            catch (InvalidOperationException)
            {
                throw CreateExpectedChatFault(LobbyUnavailableCode, nameof(JoinLobbyChat));
            }
            catch (Exception exception)
            {
                throw CreateUnexpectedChatFault(exception, nameof(JoinLobbyChat));
            }
        }

        public void SendMessage(string sessionId, string roomId, string message)
        {
            try
            {
                PlayerSession playerSession = _playerSessionManager.GetSession(sessionId, true);
                LobbyManager.SendMessage(playerSession, roomId, message);
            }
            catch (ArgumentException)
            {
                throw CreateExpectedChatFault(InvalidRequestCode, nameof(SendMessage));
            }
            catch (UnauthorizedAccessException)
            {
                throw CreateExpectedChatFault(AccessDeniedCode, nameof(SendMessage));
            }
            catch (InvalidOperationException)
            {
                throw CreateExpectedChatFault(LobbyUnavailableCode, nameof(SendMessage));
            }
            catch (Exception exception)
            {
                throw CreateUnexpectedChatFault(exception, nameof(SendMessage));
            }
        }

        public void LeaveLobbyChat(string sessionId, string roomId)
        {
            try
            {
                PlayerSession playerSession = _playerSessionManager.GetSession(sessionId, true);
                LobbyManager.LeaveChat(playerSession, roomId);
            }
            catch (ArgumentException)
            {
                throw CreateExpectedChatFault(InvalidRequestCode, nameof(LeaveLobbyChat));
            }
            catch (UnauthorizedAccessException)
            {
                throw CreateExpectedChatFault(AccessDeniedCode, nameof(LeaveLobbyChat));
            }
            catch (Exception exception)
            {
                throw CreateUnexpectedChatFault(exception, nameof(LeaveLobbyChat));
            }
        }

        private FaultException<ChatFault> CreateExpectedChatFault(string errorCode, string operationName)
        {
            ApplicationLogger.LogWarning("Chat operation {0} was rejected with code {1}.", operationName, errorCode);

            FaultException<ChatFault> faultException = CreateFault(errorCode);

            return faultException;
        }

        private FaultException<ChatFault> CreateUnexpectedChatFault(
            Exception exception,
            string operationName)
        {
            ApplicationLogger.LogError(
                exception,
                "Unexpected error during chat operation {0}.",
                operationName);

            FaultException<ChatFault> faultException = CreateFault(ServiceUnavailableCode);

            return faultException;
        }

        private FaultException<ChatFault> CreateFault(string errorCode)
        {
            ChatFault chatFault = new ChatFault();
            chatFault.ErrorCode = errorCode;

            FaultException<ChatFault> faultException =
                new FaultException<ChatFault>(chatFault, ChatOperationErrorMessage);

            return faultException;
        }
    }
}
