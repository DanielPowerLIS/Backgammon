using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class ChatFault
    {
        [DataMember]
        public string ErrorCode { get; set; }
    }
}
