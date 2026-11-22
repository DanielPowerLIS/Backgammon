using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public enum MatchEndReason
    {
        [EnumMember]
        None,

        [EnumMember]
        TimeExpired,

        [EnumMember]
        AllCheckersBorneOff
    }
}