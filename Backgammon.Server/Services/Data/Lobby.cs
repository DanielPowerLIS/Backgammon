using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class Lobby
    {
        [DataMember]
        public string RoomId { get; set; }

        [DataMember]
        public int HostPlayerId { get; set; }

        [DataMember]
        public string HostUsername { get; set; }

        [DataMember]
        public int GuestPlayerId { get; set; }

        [DataMember]
        public string GuestUsername { get; set; }

        public bool IsFull
        {
            get 
            {
                return HostPlayerId != 0 && GuestPlayerId != 0;
            }
        }
    }
}
