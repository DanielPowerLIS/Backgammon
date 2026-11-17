using System;
using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class MatchState
    {
        public MatchState()
        {
            MatchId = string.Empty;
            RoomId = string.Empty;
            Phase = MatchPhase.WaitingForPlayers;
            Board = new MatchBoardState();
            CountdownEndsAtUtc = DateTime.MinValue;
            ServerTimeUtc = DateTime.UtcNow;
        }

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
        public DateTime CountdownEndsAtUtc { get; set; }

        [DataMember]
        public DateTime ServerTimeUtc { get; set; }
    }
}