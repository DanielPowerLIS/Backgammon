using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class Account
    {
        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string PaternalSurname { get; set; }

        [DataMember]
        public string MaternalSurname { get; set; }
    }
}