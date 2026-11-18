using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class MatchOpeningRollState
    {
        private const int InitialRoundNumber = 1;

        public MatchOpeningRollState()
        {
            RoundNumber = InitialRoundNumber;
            HostDieValue = 0;
            GuestDieValue = 0;
        }

        [DataMember]
        public int RoundNumber { get; set; }

        [DataMember]
        public int HostDieValue { get; set; }

        [DataMember]
        public int GuestDieValue { get; set; }
    }
}