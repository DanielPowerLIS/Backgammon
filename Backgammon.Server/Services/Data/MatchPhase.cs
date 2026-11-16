using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public enum MatchPhase
    {
        [EnumMember]
        WaitingForPlayers,

        [EnumMember]
        StartingCountdown,

        [EnumMember]
        OpeningRoll,

        [EnumMember]
        WaitingForRoll,

        [EnumMember]
        MovingCheckers,

        [EnumMember]
        Finished
    }
}