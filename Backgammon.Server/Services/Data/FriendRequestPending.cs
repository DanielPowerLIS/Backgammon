using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class FriendRequestPending
    {
        [DataMember]
        public int RequestId { get; set; }

        [DataMember]
        public string SenderUsername { get; set; }
    }
}
