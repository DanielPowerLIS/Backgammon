using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System;

using Backgammon.Server.SupportServices;
using Backgammon.Server.Persistence.Data;

namespace Backgammon.Server.Persistence.Repositories
{
    public class AccountRepository
    {
        private static readonly int MaximumCodeAttempts = 10;

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

        public async Task<bool> IsEmailRegisteredAsync(string emailAddress)
        {
            using (var databaseContext = new BackgammonEntities())
            {
                bool emailRegistered = await databaseContext.Accounts
                    .AnyAsync(playerAccount => playerAccount.Email == emailAddress);

                return emailRegistered;
            }
        }

        public async Task<bool> IsUsernameRegisteredAsync(string playerUsername)
        {
            using (var databaseContext = new BackgammonEntities())
            {
                bool usernameRegistered = await databaseContext.Profiles
                    .AnyAsync(playerProfile => playerProfile.Username == playerUsername);

                return usernameRegistered;
            }
        }

        public async Task CreateAccountAsync(Account playerAccount)
        {
            using (var databaseContext = new BackgammonEntities())
            {
                playerAccount.AccountCode =
                    await GenerateAvailableCodeAsync(databaseContext);

                databaseContext.Accounts.Add(playerAccount);

                await databaseContext.SaveChangesAsync();
            }
        }

        private async Task<string> GenerateAvailableCodeAsync(
            BackgammonEntities databaseContext)
        {
            var codeGenerator = new AccountCodeGenerator();

            for (int codeAttempt = 0; codeAttempt < MaximumCodeAttempts; codeAttempt++)
            {
                string accountCode = codeGenerator.Generate();

                bool codeRegistered = await databaseContext.Accounts
                    .AnyAsync(playerAccount => playerAccount.AccountCode == accountCode);

                if (!codeRegistered)
                {
                    return accountCode;
                }
            }

            throw new InvalidOperationException(
                "Could not generate an available account code.");
        }
    }
}