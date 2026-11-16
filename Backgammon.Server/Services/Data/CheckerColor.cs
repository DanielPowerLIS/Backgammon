using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public enum CheckerColor
    {
        [EnumMember]
        None,

        [EnumMember]
        Blue,

        [EnumMember]
        Pink
    }
}