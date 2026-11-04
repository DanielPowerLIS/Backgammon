using System;
using System.Collections.Generic;
using System.ServiceModel;

using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Services.Data;
using Backgammon.Server.SupportServices;
using Backgammon.Server.SupportServices.Logging;

namespace Backgammon.Server.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class ChatService : IChatService
    {
        private readonly PlayerSessionManager _playerSessionManager = new PlayerSessionManager();

        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(ChatService));

        private static readonly string ChatOperationErrorMessage = "The chat operation could not be completed.";

        public List<ChatMessage> JoinLobbyChat(string sessionId, string roomId)
        {
            PlayerSession playerSession = GetPlayerSession(sessionId);
            IChatCallback chatCallback = OperationContext.Current.GetCallbackChannel<IChatCallback>();

            try
            {
                List<ChatMessage> chatMessages = LobbyManager.JoinChat(playerSession, roomId, chatCallback);

                return chatMessages;
            }
            catch (ArgumentException chatException)
            {
                throw CreateChatFault(chatException, nameof(JoinLobbyChat));
            }
            catch (UnauthorizedAccessException chatException)
            {
                throw CreateChatFault(chatException, nameof(JoinLobbyChat));
            }
            catch (InvalidOperationException chatException)
            {
                throw CreateChatFault(chatException, nameof(JoinLobbyChat));
            }
        }

        public void SendMessage(string sessionId, string roomId, string message)
        {
            PlayerSession playerSession = GetPlayerSession(sessionId);

            try
            {
                LobbyManager.SendMessage(playerSession, roomId, message);
            }
            catch (ArgumentException chatException)
            {
                throw CreateChatFault(chatException, nameof(SendMessage));
            }
            catch (UnauthorizedAccessException chatException)
            {
                throw CreateChatFault(chatException, nameof(SendMessage));
            }
            catch (InvalidOperationException chatException)
            {
                throw CreateChatFault(chatException, nameof(SendMessage));
            }
        }

        public void LeaveLobbyChat(string sessionId, string roomId)
        {
            PlayerSession playerSession = GetPlayerSession(sessionId);

            try
            {
                LobbyManager.LeaveChat(playerSession, roomId);
            }
            catch (UnauthorizedAccessException chatException)
            {
                throw CreateChatFault(chatException, nameof(LeaveLobbyChat));
            }
            catch (ArgumentNullException chatException)
            {
                throw CreateChatFault(chatException, nameof(LeaveLobbyChat));
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
                throw CreateChatFault(sessionException, nameof(GetPlayerSession));
            }
        }

        private FaultException CreateChatFault(Exception chatException, string operationName)
        {
            ApplicationLogger.LogWarning("The chat operation {0} was rejected. Reason: {1}", operationName,
                chatException.Message);

            var chatFault = new FaultException(new FaultReason(ChatOperationErrorMessage));

            return chatFault;
        }
    }
}
