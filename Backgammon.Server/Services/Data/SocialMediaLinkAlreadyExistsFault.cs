using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class SocialMediaLinkAlreadyExistsFault
    {
        [DataMember]
        public string Message { get; set; }
    }
}