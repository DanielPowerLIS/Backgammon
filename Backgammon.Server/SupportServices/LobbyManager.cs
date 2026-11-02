using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ServiceModel;

using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Services.Data;
using Backgammon.Server.SupportServices.Logging;

namespace Backgammon.Server.SupportServices
{
    internal static class LobbyManager
    {
        private static readonly int RoomCodeLength = 6;
        private static readonly int MaximumMessageLength = 200;

        private static readonly string CodeLobbyDoesNotExistMessage = "The lobby code does not exist.";
        private static readonly string LobbyFullMessage = "The lobby is already full.";
        private static readonly string HostCannotJoinMessage = "The host cannot join their own lobby.";
        private static readonly string InvalidMessage = "The chat message is invalid or exceeds the maximum length.";
        private static readonly string PlayerNotBelongLobby = "The player does not belong to this lobby.";

        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(LobbyManager));

        private static readonly ConcurrentDictionary<string, LobbySession> ActiveRooms =
            new ConcurrentDictionary<string, LobbySession>();

        public static Lobby CreateRoom(PlayerSession hostPlayer, IPreinitiationCallback hostCallback)
        {
            ValidateCreateRoomArguments(hostPlayer, hostCallback);

            Lobby lobbyData = CreateLobbyData(hostPlayer);
            var lobbySession = new LobbySession
            {
                RoomData = lobbyData,
                HostCallback = hostCallback
            };

            AddRoom(lobbySession);

            Lobby createdLobby = lobbyData;

            return createdLobby;
        }

        public static LobbySession JoinRoom(PlayerSession guestPlayer, string roomId,
            IPreinitiationCallback guestCallback)
        {
            ValidateJoinRoomArguments(guestPlayer, roomId, guestCallback);

            LobbySession lobbySession = GetLobbySession(roomId);

            lock (lobbySession.Queue)
            {
                ValidateGuestCanJoin(lobbySession, guestPlayer);
                AddGuestToLobby(lobbySession, guestPlayer, guestCallback);
            }

            LobbySession joinedLobbySession = lobbySession;

            return joinedLobbySession;
        }

        public static void CancelJoin(LobbySession lobbySession, int guestPlayerId)
        {
            if (lobbySession == null)
            {
                throw new ArgumentNullException(nameof(lobbySession));
            }

            lock (lobbySession.Queue)
            {
                ClearGuestIfMatches(lobbySession, guestPlayerId);
            }
        }

        public static List<ChatMessage> JoinChat(PlayerSession playerSession, string roomId, IChatCallback chatCallback)
        {
            ValidateJoinChatArguments(playerSession, chatCallback);

            LobbySession lobbySession = GetLobbySession(roomId);
            List<ChatMessage> chatMessages;

            lock (lobbySession.Queue)
            {
                RegisterChatCallback(lobbySession, playerSession, chatCallback);
                chatMessages = new List<ChatMessage>(lobbySession.ChatMessages);
            }

            return chatMessages;
        }

        public static ChatMessage SendMessage(PlayerSession sender, string roomId, string message)
        {
            if (sender == null)
            {
                throw new ArgumentNullException(nameof(sender));
            }

            string validMessage = ValidateMessage(message);
            LobbySession lobbySession = GetLobbySession(roomId);
            ChatMessage chatMessage;
            List<IChatCallback> callbacks;

            lock (lobbySession.Queue)
            {
                chatMessage = CreateChatMessage(lobbySession, sender, validMessage);
                lobbySession.ChatMessages.Add(chatMessage);
                callbacks = GetChatCallbacks(lobbySession);
            }

            NotifyChatMessage(lobbySession, callbacks, chatMessage);

            ChatMessage sentMessage = chatMessage;

            return sentMessage;
        }

        public static void LeaveChat(PlayerSession playerSession, string roomId)
        {
            if (playerSession == null)
            {
                throw new ArgumentNullException(nameof(playerSession));
            }

            if (roomId == null)
            {
                throw new ArgumentNullException(nameof(roomId));
            }

            if (!ActiveRooms.TryGetValue(roomId, out LobbySession lobbySession))
            {
                return;
            }

            lock (lobbySession.Queue)
            {
                RemoveChatCallback(lobbySession, playerSession.PlayerId);
            }
        }

        private static void ValidateCreateRoomArguments(PlayerSession hostPlayer, IPreinitiationCallback hostCallback)
        {
            if (hostPlayer == null)
            {
                throw new ArgumentNullException(nameof(hostPlayer));
            }

            if (hostCallback == null)
            {
                throw new ArgumentNullException(nameof(hostCallback));
            }
        }

        private static Lobby CreateLobbyData(PlayerSession hostPlayer)
        {
            var lobbyData = new Lobby
            {
                HostPlayerId = hostPlayer.PlayerId,
                HostUsername = hostPlayer.Username,
                GuestUsername = string.Empty
            };

            Lobby createdLobbyData = lobbyData;

            return createdLobbyData;
        }

        private static void AddRoom(LobbySession lobbySession)
        {
            var codeGenerator = new CodeGenerator();
            bool roomWasAdded = false;

            while (!roomWasAdded)
            {
                string roomId = codeGenerator.Generate(RoomCodeLength);
                lobbySession.RoomData.RoomId = roomId;
                roomWasAdded = ActiveRooms.TryAdd(roomId, lobbySession);
            }
        }

        private static void ValidateJoinRoomArguments(PlayerSession guestPlayer, string roomId,
            IPreinitiationCallback guestCallback)
        {
            if (guestPlayer == null)
            {
                throw new ArgumentNullException(nameof(guestPlayer));
            }

            if (roomId == null)
            {
                throw new ArgumentNullException(nameof(roomId));
            }

            if (guestCallback == null)
            {
                throw new ArgumentNullException(nameof(guestCallback));
            }
        }

        private static LobbySession GetLobbySession(string roomId)
        {
            if (roomId == null)
            {
                throw new ArgumentNullException(nameof(roomId));
            }

            if (!ActiveRooms.TryGetValue(roomId, out LobbySession lobbySession))
            {
                throw new InvalidOperationException(CodeLobbyDoesNotExistMessage);
            }

            LobbySession foundLobbySession = lobbySession;

            return foundLobbySession;
        }

        private static void ValidateGuestCanJoin(LobbySession lobbySession, PlayerSession guestPlayer)
        {
            if (lobbySession.RoomData.HostPlayerId == guestPlayer.PlayerId)
            {
                throw new InvalidOperationException(HostCannotJoinMessage);
            }

            if (lobbySession.RoomData.IsFull)
            {
                throw new InvalidOperationException(LobbyFullMessage);
            }
        }

        private static void AddGuestToLobby(LobbySession lobbySession, PlayerSession guestPlayer,
            IPreinitiationCallback guestCallback)
        {
            lobbySession.RoomData.GuestPlayerId = guestPlayer.PlayerId;
            lobbySession.RoomData.GuestUsername = guestPlayer.Username;
            lobbySession.GuestCallback = guestCallback;
        }

        private static void ClearGuestIfMatches(LobbySession lobbySession, int guestPlayerId)
        {
            if (lobbySession.RoomData.GuestPlayerId != guestPlayerId)
            {
                return;
            }

            lobbySession.RoomData.GuestPlayerId = 0;
            lobbySession.RoomData.GuestUsername = string.Empty;
            lobbySession.GuestCallback = null;
            lobbySession.GuestChatCallback = null;
        }

        private static void ValidateJoinChatArguments(PlayerSession playerSession, IChatCallback chatCallback)
        {
            if (playerSession == null)
            {
                throw new ArgumentNullException(nameof(playerSession));
            }

            if (chatCallback == null)
            {
                throw new ArgumentNullException(nameof(chatCallback));
            }
        }

        private static void RegisterChatCallback(LobbySession lobbySession, PlayerSession playerSession,
            IChatCallback chatCallback)
        {
            if (lobbySession.RoomData.HostPlayerId == playerSession.PlayerId)
            {
                lobbySession.HostChatCallback = chatCallback;
            }
            else if (lobbySession.RoomData.GuestPlayerId == playerSession.PlayerId)
            {
                lobbySession.GuestChatCallback = chatCallback;
            }
            else
            {
                throw new UnauthorizedAccessException(PlayerNotBelongLobby);
            }
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

        private static ChatMessage CreateChatMessage(LobbySession lobbySession, PlayerSession sender, string message)
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

        private static string GetPlayerUsername(
            LobbySession lobbySession,
            int playerId)
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

        private static void NotifyChatMessage(LobbySession lobbySession, List<IChatCallback> callbacks,
            ChatMessage chatMessage)
        {
            foreach (IChatCallback callback in callbacks)
            {
                TryNotifyChatMessage(lobbySession, callback, chatMessage);
            }
        }

        private static void TryNotifyChatMessage(LobbySession lobbySession, IChatCallback chatCallback,
            ChatMessage chatMessage)
        {
            try
            {
                chatCallback.OnMessageReceived(chatMessage);
            }
            catch (CommunicationException communicationException)
            {
                HandleChatCallbackFailure(
                    lobbySession, chatCallback, communicationException);
            }
            catch (TimeoutException timeoutException)
            {
                HandleChatCallbackFailure(
                    lobbySession, chatCallback, timeoutException);
            }
            catch (ObjectDisposedException disposedException)
            {
                HandleChatCallbackFailure(
                    lobbySession, chatCallback, disposedException);
            }
        }

        private static void HandleChatCallbackFailure(LobbySession lobbySession, IChatCallback chatCallback,
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
            else if (ReferenceEquals(lobbySession.GuestChatCallback, chatCallback))
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
