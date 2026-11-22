using System;
using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class MatchClockState
    {
        private const int InitialGlobalMinutes = 2;

        [DataMember]
        public TimeSpan HostRemainingTime { get; set; }

        [DataMember]
        public TimeSpan GuestRemainingTime { get; set; }

        [DataMember]
        public TimeSpan GraceRemainingTime { get; set; }

        [DataMember]
        public DateTime ServerTimeUtc { get; set; }

        [DataMember]
        public bool IsRunning { get; set; }

        public MatchClockState()
        {
            HostRemainingTime = TimeSpan.FromMinutes(InitialGlobalMinutes);
            GuestRemainingTime = TimeSpan.FromMinutes(InitialGlobalMinutes);
            GraceRemainingTime = TimeSpan.Zero;
            ServerTimeUtc = DateTime.UtcNow;
            IsRunning = false;
        }
    }
}