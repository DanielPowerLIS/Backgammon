using System;
using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class MatchState
    {
        private const int UnassignedPlayerId = 0;

        [DataMember]
        public string MatchId { get; set; }

        [DataMember]
        public string RoomId { get; set; }

        [DataMember]
        public MatchPhase Phase { get; set; }

        [DataMember]
        public MatchBoardState Board { get; set; }

        [DataMember]
        public long Version { get; set; }

        [DataMember]
        public int CurrentPlayerId { get; set; }

        [DataMember]
        public MatchDiceState Dice { get; set; }

        [DataMember]
        public CheckerMove[] AvailableMoves { get; set; }

        public MatchState()
        {
            MatchId = string.Empty;
            RoomId = string.Empty;
            Phase = MatchPhase.WaitingForPlayers;
            Board = new MatchBoardState();
            CurrentPlayerId = UnassignedPlayerId;
            Dice = new MatchDiceState();
            AvailableMoves = Array.Empty<CheckerMove>();
        }
    }
}