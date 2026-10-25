using System;
using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class PlayerProfile
    {
        [DataMember]
        public int Points { get; set; }

        [DataMember]
        public int Wins { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public DateTime RegistrationDate { get; set; }

        [DataMember]
        public string FacebookLink { get; set; }

        [DataMember]
        public string InstagramLink { get; set; }

        [DataMember]
        public string TikTokLink { get; set; }

        [DataMember]
        public Avatar Avatar { get; set; }
    }
}