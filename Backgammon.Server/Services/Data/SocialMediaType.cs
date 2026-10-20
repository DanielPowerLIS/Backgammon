using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class SocialMediaType
    {
        [DataMember]
        public int IdType { get; set; }

        [DataMember]
        public string Type { get; set; }
    }
}