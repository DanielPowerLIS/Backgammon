using System;
using System.Collections.Concurrent;

using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Services.Data;

namespace Backgammon.Server.SupportServices
{
    internal static class LobbyManager
    {
        private const int RoomCodeLength = 6;

        private static readonly string CodeLobbyDoesNotExistMessage = "The lobby code does not exist.";
        private static readonly string LobbyFullMessage = "The lobby is already full.";
        private static readonly string HostCannotJoinMessage = "The host cannot join their own lobby.";


        private static readonly ConcurrentDictionary<string, LobbySession> ActiveRooms =
            new ConcurrentDictionary<string, LobbySession>();

        public static Lobby CreateRoom(int hostPlayerId, IPreinitiationCallback hostCallback) 
        {
            var codeGenerator = new CodeGenerator();

            string roomId = codeGenerator.Generate(RoomCodeLength);

            Lobby lobbyData = new Lobby
            {
                RoomId = roomId,
                HostPlayerId = hostPlayerId,
            };

            LobbySession session = new LobbySession
            {
                RoomData = lobbyData,
                HostCallback = hostCallback
            };

            ActiveRooms.TryAdd(roomId, session);

            return lobbyData;
        }


        public static LobbySession JoinRoom(int guestId, string roomId, IPreinitiationCallback guestCallback)
        {
            if (!ActiveRooms.TryGetValue(roomId, out LobbySession session))
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
