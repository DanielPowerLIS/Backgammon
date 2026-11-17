using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class Friend
    {
        [DataMember]
        public int AccountId { get; set; }

        [DataMember]
        public string Username { get; set; }

    }
}
