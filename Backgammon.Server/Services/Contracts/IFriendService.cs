using Backgammon.Server.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Backgammon.Server.Services.Contracts
{
    [ServiceContract]
    public interface IFriendService
    {
        [OperationContract]
        [FaultContract(typeof(FriendFault))]
        List<Friend> GetFriends(string userId);
    }
}
