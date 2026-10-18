using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class RegistrationFault
    {
        [DataMember]
        public string ErrorCode { get; set; }
    }
}