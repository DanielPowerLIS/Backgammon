using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class MatchState
    {
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

        public MatchState()
        {
            MatchId = string.Empty;
            RoomId = string.Empty;
            Phase = MatchPhase.WaitingForPlayers;
            Board = new MatchBoardState();
            Turn = new MatchTurnState();
        }
    }
}