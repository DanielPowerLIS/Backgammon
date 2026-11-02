

using System;
using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class ChatMessage
    {
        [DataMember]
        public int SenderPlayerId { get; set; }

        [DataMember]
        public string SenderUsername { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public DateTime SendAtUtc { get; set; } 
    }
}
