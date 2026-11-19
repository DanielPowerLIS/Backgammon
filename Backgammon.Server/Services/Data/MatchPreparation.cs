using System;
using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class MatchPreparation
    {
        private const long InitialVersion = 1;

        [DataMember]
        public string MatchId { get; set; }

        [DataMember]
        public string RoomId { get; set; }

        [DataMember]
        public MatchPhase Phase { get; set; }

        [DataMember]
        public long Version { get; set; }

        [DataMember]
        public DateTime CountdownEndsAtUtc { get; set; }

        [DataMember]
        public DateTime ServerTimeUtc { get; set; }

        [DataMember]
        public MatchOpeningRollState OpeningRoll { get; set; }

        public MatchPreparation()
        {
            MatchId = string.Empty;
            RoomId = string.Empty;
            Phase = MatchPhase.WaitingForPlayers;
            Version = InitialVersion;
            CountdownEndsAtUtc = DateTime.MinValue;
            ServerTimeUtc = DateTime.UtcNow;
            OpeningRoll = new MatchOpeningRollState();
        }
    }
}