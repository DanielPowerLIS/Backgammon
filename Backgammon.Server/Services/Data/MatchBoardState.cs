using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class MatchBoardState
    {
        private static readonly int TotalPoints = 24;

        public MatchBoardState()
        {
            Points = new MatchPointState[TotalPoints];

            for (int pointIndex = 0; pointIndex < TotalPoints; pointIndex++)
            {
                Points[pointIndex] = new MatchPointState
                {
                    Number = pointIndex + 1,
                    Owner = CheckerColor.None,
                    Count = 0
                };
            }
        }

        [DataMember]
        public MatchPointState[] Points { get; set; }

        [DataMember]
        public int BlueOnBar { get; set; }

        [DataMember]
        public int PinkOnBar { get; set; }

        [DataMember]
        public int BlueBorneOff { get; set; }

        [DataMember]
        public int PinkBorneOff { get; set; }
    }
}