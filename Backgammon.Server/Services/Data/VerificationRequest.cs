using System;
using System.Runtime.Serialization;

using Backgammon.Server.SupportServices;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class VerificationRequest
    {
        [DataMember]
        public string RequestId { get; set; }

        public DateTime ExpiresAtUtc { get; set; }

        public byte[] CodeHash { get; set; }

        public int RemainingAttempts { get; set; }

        public int AccountId { get; set; }

        internal Entrance Queue { get; } = new Entrance();
    }
}