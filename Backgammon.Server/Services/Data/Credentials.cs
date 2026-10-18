using System;
using System.Runtime.Serialization;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class Credentials
    {
        [DataMember]
        public String Identifier { get; set; }

        [DataMember]
        public String Password { get; set; }
    }
}
