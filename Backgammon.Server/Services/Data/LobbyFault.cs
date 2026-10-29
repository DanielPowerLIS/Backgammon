using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class LobbyFault
    {
        [DataMember]
        public string ErrorCode { get; set; }

    }
}
