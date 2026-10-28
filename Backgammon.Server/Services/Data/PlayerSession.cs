using System;
using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class PlayerSession
    {
        [DataMember]
        public string SessionId { get; set; }

        [DataMember]
        public int PlayerId { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public DateTime ExpiresAtUtc { get; set; }

        [DataMember]
        public bool IsGuest { get; set; }

        [DataMember]
        public bool IsPlaying { get; set; }
    }
}
