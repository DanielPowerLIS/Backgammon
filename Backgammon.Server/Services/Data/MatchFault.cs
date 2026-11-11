
using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class MatchFault
    {
        [DataMember]
        public string ErrorCode { get; set; }
    }
}
