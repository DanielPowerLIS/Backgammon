using System;
using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class MatchTurnState
    {
        private const int UnassignedPlayerId = 0;

        [DataMember]
        public int CurrentPlayerId { get; set; }

        [DataMember]
        public MatchDiceState Dice { get; set; }

        [DataMember]
        public CheckerMove[] AvailableMoves { get; set; }

        public MatchTurnState()
        {
            CurrentPlayerId = UnassignedPlayerId;
            Dice = new MatchDiceState();
            AvailableMoves = Array.Empty<CheckerMove>();
        }
    }
}