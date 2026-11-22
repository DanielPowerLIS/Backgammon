using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class MatchResult
    {
        private const int UnassignedPlayerId = 0;

        [DataMember]
        public int WinnerPlayerId { get; set; }

        [DataMember]
        public int LoserPlayerId { get; set; }

        [DataMember]
        public MatchEndReason Reason { get; set; }

        public MatchResult()
        {
            WinnerPlayerId = UnassignedPlayerId;
            LoserPlayerId = UnassignedPlayerId;
            Reason = MatchEndReason.None;
        }
    }
}