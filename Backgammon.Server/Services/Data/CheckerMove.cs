using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class CheckerMove
    {
        public CheckerMove()
        {
            SourcePointNumber = 0;
            DestinationPointNumber = 0;
            FirstDieUses = 0;
            SecondDieUses = 0;
        }

        [DataMember]
        public int SourcePointNumber { get; set; }

        [DataMember]
        public int DestinationPointNumber { get; set; }

        [DataMember]
        public int FirstDieUses { get; set; }

        [DataMember]
        public int SecondDieUses { get; set; }
    }
}