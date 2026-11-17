using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

using Backgammon.Server.Persistence.Data;
using Backgammon.Server.Persistence.Repositories.InterfacesRepositories;

namespace Backgammon.Server.Persistence.Repositories
{
    public class FriendRepository : IFriendRepository
    {
        public async Task<List<Account>> GetFriendsAsync(int profileId)
        {
            List<Account> friendAccounts;

            using (var databaseContext = new BackgammonEntities())
            {
                int accountId = await (from account in databaseContext.Accounts
                    where account.IdProfileFK == profileId
                    select account.IdAccountPK)
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(false);

                if (accountId <= 0)
                {
                    friendAccounts = new List<Account>();
                    return friendAccounts;
                }

                IQueryable<int> friendsFromFirstSide = from friendship in databaseContext.FriendShips
                    where friendship.AccountOneId == accountId
                        && friendship.AccountTwoId != accountId
                    select friendship.AccountTwoId;

                IQueryable<int> friendsFromSecondSide = from friendship in databaseContext.FriendShips 
                    where friendship.AccountTwoId == accountId
                        && friendship.AccountOneId != accountId
                    select friendship.AccountOneId;

                List<int> friendAccountIds = await friendsFromFirstSide.Union(friendsFromSecondSide).ToListAsync()
                    .ConfigureAwait(false);

                IQueryable<Account> friendAccountQuery = from account in databaseContext.Accounts
                    where friendAccountIds.Contains(account.IdAccountPK)
                    select account;

                friendAccounts = await friendAccountQuery
                    .Include("Profile")
                    .AsNoTracking()
                    .ToListAsync()
                    .ConfigureAwait(false);
            }

            return friendAccounts;
        }
    }
}