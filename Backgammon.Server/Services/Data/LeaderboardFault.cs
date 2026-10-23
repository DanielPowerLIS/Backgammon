using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class LeaderboardFault
    {
        [DataMember]
        public string ErrorCode { get; set; }
    }
}
