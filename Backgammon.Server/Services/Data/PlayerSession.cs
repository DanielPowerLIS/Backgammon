using System;
using System.Runtime.Serialization;

using Backgammon.Server.SupportServices;

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

        internal Entrance Queue { get; } = new Entrance();
    }
}
