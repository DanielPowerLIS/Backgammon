
using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]  
    public class ProfileNotFoundFault
    {
        [DataMember]
        public string ErrorCode { get; set; }
    }
}
