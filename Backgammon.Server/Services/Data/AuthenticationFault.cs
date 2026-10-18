using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Backgammon.Server.Services.Data
{
    [DataContract]
    public class AuthenticationFault
    {
        [DataMember]
        public string ErrorCode { get; set; }
    }
}
