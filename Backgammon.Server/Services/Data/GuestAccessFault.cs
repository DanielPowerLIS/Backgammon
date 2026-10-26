using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class GuestAccessFault
    {
        [DataMember]
        public string ErrorCode { get; set; }
    }
}