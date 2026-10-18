using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Responses
{
    [DataContract]
    public class PlayerStanding
    {
        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public int Wins { get; set; }

        [DataMember]
        public int Points { get; set; }
    }
}
