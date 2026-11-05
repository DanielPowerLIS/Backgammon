using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Services.Data;
using Backgammon.Server.SupportServices.Logging;

namespace Backgammon.Server.SupportServices
{
    internal static class LobbyManager
    {
        private static readonly int RoomCodeLength = 6;

        private static readonly string CodeLobbyDoesNotExistMessage = "The lobby code does not exist.";
        private static readonly string LobbyFullMessage = "The lobby is already full.";
        private static readonly string HostCannotJoinMessage = "The host cannot join their own lobby.";

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
            LobbySession lobbySession = GetLobbySession(roomId);
            List<ChatMessage> chatMessages = ChatManager.JoinChat(
                lobbySession,
                playerSession,
                chatCallback);

            return chatMessages;
        }

        public static ChatMessage SendMessage(PlayerSession sender, string roomId, string message)
        {
            LobbySession lobbySession = GetLobbySession(roomId);
            ChatMessage sentMessage = ChatManager.SendMessage(
                lobbySession,
                sender,
                message);

            return sentMessage;
        }

        public static void LeaveChat(PlayerSession playerSession, string roomId)
        {
            LobbySession lobbySession;

            try
            {
                lobbySession = GetLobbySession(roomId);
            }
            catch (InvalidOperationException lobbyException)
            {
                ApplicationLogger.LogWarning(
                    "Chat departure was skipped because the lobby no longer exists. Reason: {0}",
                    lobbyException.Message);

                return;
            }

            ChatManager.LeaveChat(lobbySession, playerSession);
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

            bool lobbyFound = ActiveRooms.TryGetValue(roomId, out LobbySession lobbySession);

            if (!lobbyFound || lobbySession == null)
            {
                throw new InvalidOperationException(CodeLobbyDoesNotExistMessage);
            }

            return lobbySession;
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

    }
}
