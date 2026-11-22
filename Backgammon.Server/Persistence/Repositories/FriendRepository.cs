using Backgammon.Server.Persistence.Data;
using Backgammon.Server.Persistence.Repositories.InterfacesRepositories;
using System;
using System.Collections.Generic;
using System.Data;
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

        public async Task RemoveFriendAsync(int profileId, int friendAccountId)
        {
            if (friendAccountId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(friendAccountId));
            }

            using (var databaseContext = new BackgammonEntities())
            {
                int ownAccountId = await databaseContext.Accounts
                    .Where(account => account.IdProfileFK == profileId)
                    .Select(account => account.IdAccountPK)
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(false);

                if (ownAccountId <= 0)
                {
                    throw new KeyNotFoundException("The requesting account was not found.");
                }

                int firstId = Math.Min(ownAccountId, friendAccountId);
                int secondId = Math.Max(ownAccountId, friendAccountId);

                FriendShip friendship = await databaseContext.FriendShips
                    .FirstOrDefaultAsync(item =>
                        item.AccountOneId == firstId &&
                        item.AccountTwoId == secondId)
                    .ConfigureAwait(false);

                if (friendship == null)
                {
                    throw new KeyNotFoundException("The friendship was not found.");
                }

                databaseContext.FriendShips.Remove(friendship);
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

        public async Task SendFriendRequestAsync(int profileId, string identifier, bool byEmail)
        {
            string value = NormalizeIdentifier(identifier, byEmail);

            using (var databaseContext = new BackgammonEntities())
            using (var transaction = databaseContext.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                Account sender = await databaseContext.Accounts
                    .SingleOrDefaultAsync(account => account.IdProfileFK == profileId)
                    .ConfigureAwait(false);

                if (sender == null)
                {
                    throw new KeyNotFoundException("The requesting account was not found.");
                }

                Account recipient = await FindRecipientAsync(databaseContext, value, byEmail)
                    .ConfigureAwait(false);

                await ValidateRelationshipAsync(
                    databaseContext, sender.IdAccountPK, recipient.IdAccountPK).ConfigureAwait(false);

                var request = new FriendRequest
                {
                    IssuingAccountFK = sender.IdAccountPK,
                    ReceivingAccountFK = recipient.IdAccountPK
                };

                databaseContext.FriendRequests.Add(request);
                await databaseContext.SaveChangesAsync().ConfigureAwait(false);

                transaction.Commit();
            }
        }

        private string NormalizeIdentifier(string identifier, bool byEmail)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                throw new ArgumentException("An account identifier is required.", nameof(identifier));
            }

            string value = identifier.Trim();

            if (byEmail && (value.Length > 100 || !value.Contains("@")))
            {
                throw new ArgumentException("The email identifier is invalid.", nameof(identifier));
            }

            if (!byEmail)
            {
                if (value.Length != 8)
                {
                    throw new ArgumentException("The account code is invalid.", nameof(identifier));
                }

                value = value.ToUpperInvariant();
            }

            return value;
        }

        private async Task<Account> FindRecipientAsync( 
            BackgammonEntities databaseContext, string identifier, bool byEmail)
        {
            IQueryable<Account> accounts = databaseContext.Accounts;

            if (byEmail)
            {
                accounts = accounts.Where(account => account.Email == identifier);
            }
            else
            {
                accounts = accounts.Where(account => account.AccountCode == identifier);
            }

            Account recipient = await accounts.FirstOrDefaultAsync().ConfigureAwait(false);

            if (recipient == null)
            {
                throw new KeyNotFoundException("The recipient account was not found.");
            }

            return recipient;
        }

        private async Task ValidateRelationshipAsync(BackgammonEntities databaseContext, int senderId, int recipientId)
        {
            if (senderId == recipientId)
            {
                throw new ArgumentException("A player cannot send a request to their own account.", nameof(recipientId));
            }

            int firstId = Math.Min(senderId, recipientId);
            int secondId = Math.Max(senderId, recipientId);

            bool areFriends = await databaseContext.FriendShips.AnyAsync(friendship =>
                friendship.AccountOneId == firstId &&
                friendship.AccountTwoId == secondId).ConfigureAwait(false);

            bool requestExists = await databaseContext.FriendRequests.AnyAsync(request =>
                (request.IssuingAccountFK == senderId && request.ReceivingAccountFK == recipientId) ||
                (request.IssuingAccountFK == recipientId && request.ReceivingAccountFK == senderId))
                .ConfigureAwait(false);

            if (areFriends || requestExists)
            {
                throw new ArgumentException("A friendship or request already exists.", nameof(recipientId));
            }
        }

    }
}
