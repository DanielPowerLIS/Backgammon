using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class MatchState
    {
        private static readonly int InitialSharedPot = 0;
        private const long InitialCaptureNumber = 0;

        [DataMember]
        public string MatchId { get; set; }

        [DataMember]
        public string RoomId { get; set; }

        [DataMember]
        public MatchPhase Phase { get; set; }

        [DataMember]
        public MatchBoardState Board { get; set; }

        [DataMember]
        public long Version { get; set; }

        [DataMember]
        public MatchTurnState Turn { get; set; }

        [DataMember]
        public int SharedPot { get; set; }

        [DataMember]
        public long CaptureNumber { get; set; }

        [DataMember]
        public MatchClockState Clock { get; set; }

        [DataMember]
        public MatchResult Result { get; set; }

        public MatchState()
        {
            MatchId = string.Empty;
            RoomId = string.Empty;
            Phase = MatchPhase.WaitingForPlayers;
            Board = new MatchBoardState();
            Turn = new MatchTurnState();
            SharedPot = InitialSharedPot;
            CaptureNumber = InitialCaptureNumber;
            Clock = new MatchClockState();
            Result = new MatchResult();
        }
    }
}