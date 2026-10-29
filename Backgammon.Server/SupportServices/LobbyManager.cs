using System;
using System.Collections.Concurrent;

using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Services.Data;

namespace Backgammon.Server.SupportServices
{
    internal static class LobbyManager
    {
        private const int RoomCodeLength = 6;
        private const int StartIndex = 0;

        private static readonly string CodeLobbyDoesNotExistMessage = "The lobby code does not exist.";
        private static readonly string LobbyFullMessage = "The lobby is already full.";
        private static readonly string HostCannotJoinMessage = "The host cannot join their own lobby.";


        private static readonly ConcurrentDictionary<string, LobbyRoomSession> ActiveRooms =
            new ConcurrentDictionary<string, LobbyRoomSession>();

        public static Lobby CreateRoom(int hostPlayerId, ILobbyCallback hostCallback) 
        {
            string newGuid = Guid.NewGuid().ToString();
            string roomId = newGuid.Substring(StartIndex, RoomCodeLength).ToUpper();

            Lobby lobbyData = new Lobby
            {
                RoomId = roomId,
                HostPlayerId = hostPlayerId,
            };

            LobbyRoomSession session = new LobbyRoomSession
            {
                RoomData = lobbyData,
                HostCallback = hostCallback
            };

            ActiveRooms.TryAdd(roomId, session);

            return lobbyData;
        }


        public static LobbyRoomSession JoinRoom(int guestId, string roomId, ILobbyCallback guestCallback)
        {
            if (!ActiveRooms.TryGetValue(roomId, out LobbyRoomSession session))
            {
                throw new InvalidOperationException(CodeLobbyDoesNotExistMessage);
            }

            if (session.RoomData.IsFull)
            {
                throw new InvalidOperationException(LobbyFullMessage);
            }

            if (session.RoomData.HostPlayerId == guestId)
            {
                throw new InvalidOperationException(HostCannotJoinMessage);
            }

            session.RoomData.GuestPlayerId = guestId;
            session.GuestCallback = guestCallback;

            return session;
        }
    }
}
