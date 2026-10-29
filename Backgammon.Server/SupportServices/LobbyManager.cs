

using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Services.Data;
using System;
using System.Collections.Concurrent;

namespace Backgammon.Server.SupportServices
{
    internal static class LobbyManager
    {
        private const int RoomCodeLength = 6;
        private const int StartIndex = 0;

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
                CurrentState = LobbyState.WaitingForPlayers
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
                throw new InvalidOperationException("The lobby code does not exist.");
            }

            if (session.RoomData.IsFull)
            {
                throw new InvalidOperationException("The lobby is already full.");
            }

            if (session.RoomData.HostPlayerId == guestId)
            {
                throw new InvalidOperationException("The host cannot join their own lobby.");
            }

            session.RoomData.GuestPlayerId = guestId;
            session.RoomData.CurrentState = LobbyState.WaitingForPlayers;
            session.GuestCallback = guestCallback;

            return session;
        }
    }
}
