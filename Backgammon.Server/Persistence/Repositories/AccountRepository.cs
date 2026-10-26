using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

using Backgammon.Server.Persistence.Data;
using Backgammon.Server.Persistence.Repositories.InterfacesRepositories;
using Backgammon.Server.SupportServices;

namespace Backgammon.Server.Persistence.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private static readonly int MaximumCodeAttempts = 10;

        public async Task<List<Account>> GetAccountsByIdentifierAsync(string accountIdentifier)
        {
            bool isEmailIdentifier = accountIdentifier.Contains("@");

            using (var databaseContext = new BackgammonEntities())
            {
                List<Account> matchingAccounts = await databaseContext.Accounts
                    .AsNoTracking()
                    .Include(playerAccount => playerAccount.Profile)
                    .Where(playerAccount => isEmailIdentifier
                        ? playerAccount.Email == accountIdentifier
                        : playerAccount.Profile.Username == accountIdentifier)
                    .ToListAsync()
                    .ConfigureAwait(false);

                return matchingAccounts;
            }
        }
        public async Task<bool> IsEmailRegisteredAsync(string emailAddress)
        {
            using (var databaseContext = new BackgammonEntities())
            {
                bool emailRegistered = await databaseContext.Accounts
                    .AnyAsync(playerAccount => playerAccount.Email == emailAddress)
                    .ConfigureAwait(false);

                return emailRegistered;
            }
        }

        public async Task<bool> IsUsernameRegisteredAsync(string playerUsername)
        {
            using (var databaseContext = new BackgammonEntities())
            {
                bool usernameRegistered = await databaseContext.Profiles
                    .AnyAsync(playerProfile => playerProfile.Username == playerUsername)
                    .ConfigureAwait(false);

                return usernameRegistered;
            }
        }

        public async Task CreateAccountAsync(Account playerAccount)
        {
            using (var databaseContext = new BackgammonEntities())
            {
                playerAccount.AccountCode =
                    await GenerateAvailableCodeAsync(databaseContext)
                    .ConfigureAwait(false);

                databaseContext.Accounts.Add(playerAccount);

                await databaseContext.SaveChangesAsync()
                    .ConfigureAwait(false);
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
                    .AnyAsync(playerAccount => playerAccount.AccountCode == accountCode)
                    .ConfigureAwait(false);

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