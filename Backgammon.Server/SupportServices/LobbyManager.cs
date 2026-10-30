using System;
using System.Collections.Concurrent;

using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Services.Data;

namespace Backgammon.Server.SupportServices
{
    internal static class LobbyManager
    {
        private static readonly int RoomCodeLength = 6;

        private static readonly string CodeLobbyDoesNotExistMessage = "The lobby code does not exist.";
        private static readonly string LobbyFullMessage = "The lobby is already full.";
        private static readonly string HostCannotJoinMessage = "The host cannot join their own lobby.";

        private static readonly ConcurrentDictionary<string, LobbySession> ActiveRooms =
            new ConcurrentDictionary<string, LobbySession>();

        public static Lobby CreateRoom(PlayerSession hostPlayer, IPreinitiationCallback hostCallback)
        {
            var codeGenerator = new CodeGenerator();

            string roomId = codeGenerator.Generate(RoomCodeLength);

            Lobby lobbyData = new Lobby
            {
                RoomId = roomId,
                HostPlayerId = hostPlayer.PlayerId,
                HostUsername = hostPlayer.Username,
                GuestUsername = string.Empty
            };

            LobbySession session = new LobbySession
            {
                RoomData = lobbyData,
                HostCallback = hostCallback
            };

            ActiveRooms.TryAdd(roomId, session);

            return lobbyData;
        }

        public static LobbySession JoinRoom(PlayerSession guestPlayer, string roomId,
            IPreinitiationCallback guestCallback)
        {   

            if (!ActiveRooms.TryGetValue(roomId, out LobbySession lobbySession))
            {
                throw new InvalidOperationException(CodeLobbyDoesNotExistMessage);
            }

            lock (lobbySession.Queue)
            {
                if (lobbySession.RoomData.HostPlayerId == guestPlayer.PlayerId)
                {
                    throw new InvalidOperationException(HostCannotJoinMessage);
                }

                if (lobbySession.RoomData.IsFull)
                {
                    throw new InvalidOperationException(LobbyFullMessage);
                }

                lobbySession.RoomData.GuestPlayerId = guestPlayer.PlayerId;
                lobbySession.RoomData.GuestUsername = guestPlayer.Username;
                lobbySession.GuestCallback = guestCallback;
            }

            return lobbySession;
        }

        public static void CancelJoin(
            LobbySession lobbySession, int guestPlayerId)
        {
            lock (lobbySession.Queue)
            {
                if (lobbySession.RoomData.GuestPlayerId != guestPlayerId)
                {
                    return;
                }

                lobbySession.RoomData.GuestPlayerId = 0;
                lobbySession.RoomData.GuestUsername = string.Empty;
                lobbySession.GuestCallback = null;
            }
        }
    }
}
