using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class SocialMediaLink
    {
        [DataMember]
        public int IdSocialMedia { get; set; }

        [DataMember]
        public int IdTypeSocialMedia { get; set; }

        [DataMember]
        public string Url { get; set; }
    }
}