using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class PlayerTurn
    {
        public PlayerTurn()
        {
            RoomId = string.Empty;
            MatchId = string.Empty;
            ExpectedVersion = 0;
        }

        [DataMember]
        public string RoomId { get; set; }

        [DataMember]
        public string MatchId { get; set; }

        [DataMember]
        public long ExpectedVersion { get; set; }
    }
}