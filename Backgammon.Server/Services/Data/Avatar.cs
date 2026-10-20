using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class Avatar
    {
        [DataMember]
        public int idAvatar { get; set; }

        [DataMember]
        public byte[] ImageBytes { get; set; }
    }
}
