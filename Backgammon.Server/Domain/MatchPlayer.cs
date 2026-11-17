using System;

namespace Backgammon.Server.Domain
{
    internal class MatchPlayer
    {
        public MatchPlayer(int playerId, string roomId, string matchId)
        {
            if (string.IsNullOrWhiteSpace(roomId) || string.IsNullOrWhiteSpace(matchId))
            {
                throw new ArgumentException("Room and match identifiers are required.");
            }

            PlayerId = playerId;
            RoomId = roomId;
            MatchId = matchId;
        }

        public int PlayerId { get; }

        public string RoomId { get; }

        public string MatchId { get; }
    }
}