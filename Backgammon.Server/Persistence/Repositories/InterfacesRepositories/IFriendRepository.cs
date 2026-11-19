using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Backgammon.Server.Persistence.Data;

namespace Backgammon.Server.Persistence.Repositories.InterfacesRepositories
{
    public interface IFriendRepository
    {
        Task<List<Account>> GetFriendsAsync(int userId);

        Task<List<FriendRequest>> GetIncomingRequestsAsync(int profileId);
    }
}
