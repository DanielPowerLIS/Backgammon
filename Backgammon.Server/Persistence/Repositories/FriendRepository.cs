using Backgammon.Server.Persistence.Data;
using Backgammon.Server.Persistence.Repositories.InterfacesRepositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<List<FriendRequest>> GetIncomingRequestsAsync(int profileId)
        {
            List<FriendRequest> friendRequests = new List<FriendRequest>();

            using (var databaseContext = new BackgammonEntities())
            {
                int accountId = await(
                    from account in databaseContext.Accounts
                    where account.IdProfileFK == profileId
                    select account.IdAccountPK)
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(false);

                if (accountId > 0)
                {
                    IQueryable<FriendRequest> requestQuery =
                        from request in databaseContext.FriendRequests
                        where request.ReceivingAccountFK == accountId
                        select request;

                    friendRequests = await requestQuery
                        .Include("Account.Profile")
                        .AsNoTracking()
                        .OrderBy(request => request.IdFriendRequestPK)
                        .ToListAsync()
                        .ConfigureAwait(false);
                }
            }

            return friendRequests;
        }

        public async Task AcceptFriendRequestAsync(int profileId, int requestId)
        {
            using (var databaseContext = new BackgammonEntities())
            using (var transaction = databaseContext.Database.BeginTransaction())
            {
                FriendRequest request = await FindIncomingRequestAsync(
                    databaseContext, profileId, requestId).ConfigureAwait(false);

                int firstAccountId = Math.Min(request.IssuingAccountFK, request.ReceivingAccountFK);
                int secondAccountId = Math.Max(request.IssuingAccountFK, request.ReceivingAccountFK);

                bool friendshipExists = await databaseContext.FriendShips.AnyAsync(friendships =>
                    friendships.AccountOneId == firstAccountId &&
                    friendships.AccountTwoId == secondAccountId).ConfigureAwait(false);

                if (friendshipExists)
                {
                    throw new ArgumentException("The friendship already exists.", nameof(requestId));
                }

                var friendship = new FriendShip
                {
                    AccountOneId = firstAccountId,
                    AccountTwoId = secondAccountId
                };

                databaseContext.FriendShips.Add(friendship);
                databaseContext.FriendRequests.Remove(request);

                await databaseContext.SaveChangesAsync().ConfigureAwait(false);
                transaction.Commit();
            }
        }

        public async Task RejectFriendRequestAsync(int profileId, int requestId)
        {
            using (var databaseContext = new BackgammonEntities())
            {
                FriendRequest request = await FindIncomingRequestAsync(
                    databaseContext, profileId, requestId).ConfigureAwait(false);

                databaseContext.FriendRequests.Remove(request);

                await databaseContext.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        private async Task<FriendRequest> FindIncomingRequestAsync(
            BackgammonEntities databaseContext, int profileId, int requestId)
        {
            if (requestId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(requestId));
            }

            FriendRequest request = await databaseContext.FriendRequests
                .FirstOrDefaultAsync(item =>
                    item.IdFriendRequestPK == requestId &&
                    item.Account1.IdProfileFK == profileId)
                .ConfigureAwait(false);

            if (request == null)
            {
                throw new KeyNotFoundException("The incoming friend request was not found.");
            }

            return request;
        }

    }
}