using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class PlayerOpeningRoll
    {
        public PlayerOpeningRoll()
        {
            RoomId = string.Empty;
            MatchId = string.Empty;
            RoundNumber = 0;
        }

        [DataMember]
        public string RoomId { get; set; }

        [DataMember]
        public string MatchId { get; set; }

        [DataMember]
        public int RoundNumber { get; set; }
    }
}