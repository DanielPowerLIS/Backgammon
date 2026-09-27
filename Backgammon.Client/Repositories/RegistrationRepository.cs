using System.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Backgammon.Client.Data;
using Backgammon.Client.Services;

namespace Backgammon.Client.Repositories
{
    internal class RegistrationRepository
    {
        private static readonly int MaximumCodeAttempts = 10;
        public async Task<bool> IsEmailRegisteredAsync(string emailAddress)
        {
            using (BackgammonEntities databaseContext = new BackgammonEntities())
            {
                return await databaseContext.Accounts.AnyAsync(
                    account => account.Email == emailAddress);
            }
        }

        public async Task<bool> IsUsernameRegisteredAsync(string username)
        {
            using (BackgammonEntities databaseContext = new BackgammonEntities())
            {
                return await databaseContext.Profiles.AnyAsync(
                    profile => profile.Username == username);
            }
        }

        public async Task CreateAccountAsync(Account account)
        {
            using (BackgammonEntities databaseContext = new BackgammonEntities())
            {
                account.AccountCode =
                    await GenerateAvailableCodeAsync(databaseContext);

                databaseContext.Accounts.Add(account);
                await databaseContext.SaveChangesAsync();
            }
        }

        private async Task<string> GenerateAvailableCodeAsync(
            BackgammonEntities databaseContext)
        {
            AccountCodeGenerator generator = new AccountCodeGenerator();

            for (int attempt = 0; attempt < MaximumCodeAttempts; attempt++)
            {
                string code = generator.Generate();
                bool alreadyExists = await databaseContext.Accounts.AnyAsync(
                    account => account.AccountCode == code);

                if (!alreadyExists)
                {
                    return code;
                }
            }

            throw new InvalidOperationException(
                "Could not generate an available account code.");
        }

    }
}
