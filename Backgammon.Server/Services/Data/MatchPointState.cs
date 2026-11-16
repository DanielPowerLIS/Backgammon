using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class MatchPointState
    {
        [DataMember]
        public int Number { get; set; }

        [DataMember]
        public CheckerColor Owner { get; set; }

        [DataMember]
        public int Count { get; set; }
    }
}