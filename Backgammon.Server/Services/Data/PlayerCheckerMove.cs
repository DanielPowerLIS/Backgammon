using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class PlayerCheckerMove
    {
        private const long UnspecifiedVersion = 0;

        [DataMember]
        public string RoomId { get; set; }

        [DataMember]
        public string MatchId { get; set; }

        [DataMember]
        public long ExpectedVersion { get; set; }

        [DataMember]
        public CheckerMove Move { get; set; }

        public PlayerCheckerMove()
        {
            RoomId = string.Empty;
            MatchId = string.Empty;
            ExpectedVersion = UnspecifiedVersion;
            Move = new CheckerMove();
        }
    }
}