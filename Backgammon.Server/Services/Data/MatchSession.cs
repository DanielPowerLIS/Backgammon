using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class MatchSession
    {
        [DataMember]
        public string MatchId { get; set; }

        [DataMember]
        public string RoomId { get; set; }

        [DataMember]
        public int PlayerHostId { get; set; }

        [DataMember]
        public int PlayerGuestId { get; set; }
    }
}
