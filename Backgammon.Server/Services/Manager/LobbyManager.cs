using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Services.Data;
using Backgammon.Server.SupportServices;
using Backgammon.Server.SupportServices.Logging;

namespace Backgammon.Server.Services.Manager
{
    internal static class LobbyManager
    {
        private static readonly int RoomCodeLength = 6;

        private static readonly string CodeLobbyDoesNotExistMessage = "The lobby code does not exist.";
        private static readonly string LobbyFullMessage = "The lobby is already full.";
        private static readonly string HostCannotJoinMessage = "The host cannot join their own lobby.";

        private static readonly string HostCannotLeaveMessage = "Host departure is not available yet.";

        private static readonly string PlayerNotGuestMessage = 
            "The player does not occupy the guest position in this lobby.";

        private static readonly string PlayerNotMemberMessage = "The player does not belong to this lobby.";

        private static readonly string PlayerNotHostMessage =
            "The player does not occupy the host position in this lobby.";

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
            LobbySession joinedLobbySession;

            lock (lobbySession.Queue)
            {
                ValidateLobbyIsActive(lobbySession);

                ValidateGuestCanJoin(lobbySession, guestPlayer);
                AddGuestToLobby(lobbySession, guestPlayer, guestCallback);
                joinedLobbySession = CreateLobbySessionCopy(lobbySession);
            }

            return joinedLobbySession;
        }

        public static void CancelJoin(LobbySession lobbySession, int guestPlayerId)
        {
            if (lobbySession == null)
            {
                throw new ArgumentNullException(nameof(lobbySession));
            }

            LobbySession storedLobbySession = GetLobbySession(lobbySession.RoomData.RoomId);

            lock (storedLobbySession.Queue)
            {
                ClearGuestIfMatches(storedLobbySession, guestPlayerId);
            }
        }

        public static LobbySession LeaveRoomAsGuest(PlayerSession playerSession, string roomId)
        {
            if (playerSession == null)
            {
                throw new ArgumentNullException(nameof(playerSession));
            }

            LobbySession lobbySession = GetLobbySession(roomId);

            lock (lobbySession.Queue)
            {
                ValidateLobbyIsActive(lobbySession);
                ValidateGuestCanLeave(lobbySession, playerSession);

                ClearGuestIfMatches(lobbySession, playerSession.PlayerId);
                MatchManager.RemoveMatch(roomId);

                LobbySession lobbyUpdate = CreateLobbySessionCopy(lobbySession);

                return lobbyUpdate;
            }
        }

        public static LobbySession LeaveRoomAsHost(PlayerSession playerSession, string roomId)
        {
            if (playerSession == null)
            {
                throw new ArgumentNullException(nameof(playerSession));
            }

            LobbySession lobbySession = GetLobbySession(roomId);
            LobbySession closedLobbySession;

            lock (lobbySession.Queue)
            {
                ValidateLobbyIsActive(lobbySession);

                if (lobbySession.RoomData.HostPlayerId != playerSession.PlayerId)
                {
                    throw new UnauthorizedAccessException(PlayerNotHostMessage);
                }

                closedLobbySession = CreateLobbySessionCopy(lobbySession);

                MatchManager.RemoveMatch(roomId);
                bool roomRemoved = RemoveStoredRoom(roomId);

                if (!roomRemoved)
                {
                    throw new InvalidOperationException(CodeLobbyDoesNotExistMessage);
                }
            }

            return closedLobbySession;
        }

        public static bool IsGuestInRoom(int playerId, string roomId)
        {
            LobbySession lobbySession = GetLobbySession(roomId);
            bool isGuest;

            lock (lobbySession.Queue)
            {
                if (playerId == 0)
                {
                    throw new UnauthorizedAccessException(PlayerNotMemberMessage);
                }

                if (lobbySession.RoomData.HostPlayerId == playerId)
                {
                    isGuest = false;
                }
                else if (lobbySession.RoomData.GuestPlayerId == playerId)
                {
                    isGuest = true;
                }
                else
                {
                    throw new UnauthorizedAccessException(PlayerNotMemberMessage);
                }
            }

            return isGuest;
        }

        public static List<ChatMessage> JoinChat(PlayerSession playerSession, string roomId, IChatCallback chatCallback)
        {
            LobbySession lobbySession = GetLobbySession(roomId);
            List<ChatMessage> chatMessages = ChatManager.JoinChat(lobbySession, playerSession, chatCallback);

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

        internal static void ValidateHostMatch(int requestPlayerId, LobbySession lobbySession)
        {
            ValidateLobbyIsActive(lobbySession);

            if (lobbySession.RoomData.HostPlayerId != requestPlayerId)
            {
                throw new UnauthorizedAccessException(PlayerNotHostMessage);
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

        public static LobbySession GetLobbySession(string roomId)
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
            lobbySession.ChatMessages.Clear();
        }

        private static void ValidateGuestCanLeave(LobbySession lobbySession, PlayerSession playerSession)
        {
            int playerId = playerSession.PlayerId;

            if (lobbySession.RoomData.HostPlayerId == playerId)
            {
                throw new InvalidOperationException(HostCannotLeaveMessage);
            }

            bool occupiesGuestPosition = playerId != 0 && lobbySession.RoomData.GuestPlayerId == playerId;

            if (!occupiesGuestPosition)
            {
                throw new UnauthorizedAccessException(PlayerNotGuestMessage);
            }
        }

        private static LobbySession CreateLobbySessionCopy(LobbySession lobbySession)
        {
            var lobbyCopy = new Lobby
            {
                RoomId = lobbySession.RoomData.RoomId,
                HostPlayerId = lobbySession.RoomData.HostPlayerId,
                HostUsername = lobbySession.RoomData.HostUsername,
                GuestPlayerId = lobbySession.RoomData.GuestPlayerId,
                GuestUsername = lobbySession.RoomData.GuestUsername
            };

            LobbySession lobbySessionCopy = new LobbySession
            {
                RoomData = lobbyCopy,
                HostCallback = lobbySession.HostCallback,
                GuestCallback = lobbySession.GuestCallback,
                GuestChatCallback = lobbySession.GuestChatCallback
            };

            return lobbySessionCopy;
        }

        private static bool RemoveStoredRoom(string roomId)
        {
            bool roomRemoved = ActiveRooms.TryRemove(roomId, out LobbySession removedSession);

            return roomRemoved;
        }

        internal static void ValidateLobbyIsActive(LobbySession lobbySession)
        {
            string roomId = lobbySession.RoomData.RoomId;
            LobbySession storedLobbySession = GetLobbySession(roomId);

            bool sameLobbySession = ReferenceEquals(storedLobbySession, lobbySession);

            if (!sameLobbySession)
            {
                throw new InvalidOperationException(CodeLobbyDoesNotExistMessage);
            }
        }


    }
}
