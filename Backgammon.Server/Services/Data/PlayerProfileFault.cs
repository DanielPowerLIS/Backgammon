
using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]  
    public class PlayerProfileFault
    {
        [DataMember]
        public string ErrorCode { get; set; }
    }
}
