using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class MatchDiceState
    {
        public MatchDiceState()
        {
            FirstDieValue = 0;
            SecondDieValue = 0;
            FirstDieRemainingUses = 0;
            SecondDieRemainingUses = 0;
        }

        [DataMember]
        public int FirstDieValue { get; set; }

        [DataMember]
        public int SecondDieValue { get; set; }

        [DataMember]
        public int FirstDieRemainingUses { get; set; }

        [DataMember]
        public int SecondDieRemainingUses { get; set; }
    }
}