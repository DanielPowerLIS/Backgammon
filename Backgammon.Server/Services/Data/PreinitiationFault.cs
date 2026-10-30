using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class PreinitiationFault
    {
        [DataMember]
        public string ErrorCode { get; set; }

    }
}
