using System;
using System.Collections.Generic;
using System.ServiceModel;

using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Services.Data;
using Backgammon.Server.SupportServices.Logging;

namespace Backgammon.Server.SupportServices
{
    internal static class ChatManager
    {
        private const int MaximumMessageLength = 200;

        private const string InvalidMessage = "The chat message is invalid or exceeds the maximum length.";

        private const string PlayerNotBelongLobby = "The player does not belong to this lobby.";

        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(ChatManager));

        public static List<ChatMessage> JoinChat(LobbySession lobbySession, PlayerSession playerSession,
            IChatCallback chatCallback)
        {
            ValidateJoinChat(lobbySession, playerSession, chatCallback);

            List<ChatMessage> chatMessages;

            lock (lobbySession.Queue)
            {
                RegisterChatCallback(lobbySession, playerSession, chatCallback);
                chatMessages = new List<ChatMessage>(lobbySession.ChatMessages);
            }

            return chatMessages;
        }

        public static ChatMessage SendMessage(LobbySession lobbySession, PlayerSession sender, string message)
        {
            ValidateSendMessage(lobbySession, sender);

            string validMessage = ValidateMessage(message);
            ChatMessage chatMessage;
            List<IChatCallback> callbacks;

            lock (lobbySession.Queue)
            {
                EnsurePlayerInLobby(lobbySession, sender);
                chatMessage = BuildChatMessage(lobbySession, sender, validMessage);
                lobbySession.ChatMessages.Add(chatMessage);
                callbacks = GetChatCallbacks(lobbySession);
            }

            NotifyParticipants(lobbySession, callbacks, chatMessage);

            ChatMessage sentMessage = chatMessage;

            return sentMessage;
        }

        public static void LeaveChat(LobbySession lobbySession, PlayerSession playerSession)
        {
            if (lobbySession == null)
            {
                throw new ArgumentNullException(nameof(lobbySession));
            }

            if (playerSession == null)
            {
                throw new ArgumentNullException(nameof(playerSession));
            }

            lock (lobbySession.Queue)
            {
                RemoveChatCallback(lobbySession, playerSession.PlayerId);
            }
        }

        private static void ValidateJoinChat(LobbySession lobbySession, PlayerSession playerSession,
            IChatCallback chatCallback)
        {
            if (lobbySession == null)
            {
                throw new ArgumentNullException(nameof(lobbySession));
            }

            if (playerSession == null)
            {
                throw new ArgumentNullException(nameof(playerSession));
            }

            if (chatCallback == null)
            {
                throw new ArgumentNullException(nameof(chatCallback));
            }
        }

        private static void ValidateSendMessage(LobbySession lobbySession, PlayerSession sender)
        {
            if (lobbySession == null)
            {
                throw new ArgumentNullException(nameof(lobbySession));
            }

            if (sender == null)
            {
                throw new ArgumentNullException(nameof(sender));
            }
        }

        private static void EnsurePlayerInLobby(LobbySession lobbySession, PlayerSession playerSession)
        {
            bool isHost = lobbySession.RoomData.HostPlayerId == playerSession.PlayerId;

            bool isGuest = lobbySession.RoomData.GuestPlayerId == playerSession.PlayerId;

            if (!isHost && !isGuest)
            {
                throw new UnauthorizedAccessException(PlayerNotBelongLobby);
            }
        }

        private static void RegisterChatCallback(LobbySession lobbySession, PlayerSession playerSession,
            IChatCallback chatCallback)
        {
            EnsurePlayerInLobby(lobbySession, playerSession);

            if (lobbySession.RoomData.HostPlayerId == playerSession.PlayerId)
            {
                lobbySession.HostChatCallback = chatCallback;
                return;
            }

            lobbySession.GuestChatCallback = chatCallback;
        }

        private static string ValidateMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException(InvalidMessage, nameof(message));
            }

            string validMessage = message.Trim();

            if (validMessage.Length > MaximumMessageLength)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(message), validMessage.Length, InvalidMessage);
            }

            return validMessage;
        }

        private static ChatMessage BuildChatMessage(LobbySession lobbySession, PlayerSession sender, string message)
        {
            string senderUsername = GetPlayerUsername(lobbySession, sender.PlayerId);

            var chatMessage = new ChatMessage
            {
                SenderPlayerId = sender.PlayerId,
                SenderUsername = senderUsername,
                Message = message,
                SendAtUtc = DateTime.UtcNow
            };

            ChatMessage createdChatMessage = chatMessage;

            return createdChatMessage;
        }

        private static string GetPlayerUsername(LobbySession lobbySession, int playerId)
        {
            string playerUsername;

            if (lobbySession.RoomData.HostPlayerId == playerId)
            {
                playerUsername = lobbySession.RoomData.HostUsername;
            }
            else if (lobbySession.RoomData.GuestPlayerId == playerId)
            {
                playerUsername = lobbySession.RoomData.GuestUsername;
            }
            else
            {
                throw new UnauthorizedAccessException(PlayerNotBelongLobby);
            }

            return playerUsername;
        }

        private static List<IChatCallback> GetChatCallbacks(LobbySession lobbySession)
        {
            var callbacks = new List<IChatCallback>();

            if (lobbySession.HostChatCallback != null)
            {
                callbacks.Add(lobbySession.HostChatCallback);
            }

            if (lobbySession.GuestChatCallback != null)
            {
                callbacks.Add(lobbySession.GuestChatCallback);
            }

            return callbacks;
        }

        private static void NotifyParticipants(LobbySession lobbySession, List<IChatCallback> callbacks,
            ChatMessage chatMessage)
        {
            foreach (IChatCallback callback in callbacks)
            {
                TrySendMessage(lobbySession, callback, chatMessage);
            }
        }

        private static void TrySendMessage(LobbySession lobbySession, IChatCallback chatCallback,
            ChatMessage chatMessage)
        {
            try
            {
                chatCallback.OnMessageReceived(chatMessage);
            }
            catch (CommunicationException communicationException)
            {
                HandleCallbackFailure(
                    lobbySession,
                    chatCallback,
                    communicationException);
            }
            catch (TimeoutException timeoutException)
            {
                HandleCallbackFailure(
                    lobbySession,
                    chatCallback,
                    timeoutException);
            }
            catch (ObjectDisposedException disposedException)
            {
                HandleCallbackFailure(
                    lobbySession,
                    chatCallback,
                    disposedException);
            }
        }

        private static void HandleCallbackFailure(LobbySession lobbySession, IChatCallback chatCallback,
            Exception callbackException)
        {
            ApplicationLogger.LogError(
                callbackException,
                "A chat message could not be delivered to a lobby participant.");

            lock (lobbySession.Queue)
            {
                RemoveChatCallback(lobbySession, chatCallback);
            }
        }

        private static void RemoveChatCallback(LobbySession lobbySession, IChatCallback chatCallback)
        {
            if (ReferenceEquals(lobbySession.HostChatCallback, chatCallback))
            {
                lobbySession.HostChatCallback = null;
            }
            else if (ReferenceEquals(lobbySession.GuestChatCallback,chatCallback))
            {
                lobbySession.GuestChatCallback = null;
            }
        }

        private static void RemoveChatCallback(LobbySession lobbySession, int playerId)
        {
            if (lobbySession.RoomData.HostPlayerId == playerId)
            {
                lobbySession.HostChatCallback = null;
            }
            else if (lobbySession.RoomData.GuestPlayerId == playerId)
            {
                lobbySession.GuestChatCallback = null;
            }
        }
    }
}
