using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    internal class PlayerSessionFault
    {
        [DataMember]
        public string ErrorCode { get; set; }
    }
}
