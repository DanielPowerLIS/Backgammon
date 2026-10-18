using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

using Backgammon.Server.Persistence.Data;

namespace Backgammon.Server.Persistence.Repositories
{
    public class AccountRepository
    {
        public async Task<List<Account>> GetAccountsByIdentifierAsync(string accountIdentifier)
        {
            using (var databaseContext = new BackgammonEntities())
            {
                List<Account> matchingAccounts = await databaseContext.Accounts
                    .AsNoTracking()
                    .Include(playerAccount => playerAccount.Profile)
                    .Where(playerAccount => playerAccount.Email == accountIdentifier ||
                        playerAccount.Profile.Username == accountIdentifier)
                    .ToListAsync();

                return matchingAccounts;
            }
        }
    }
}