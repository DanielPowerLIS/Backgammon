using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

using Backgammon.Client.Utils;
using Backgammon.Client.Data;
using Backgammon.Client.Services;

namespace Backgammon.Client.Repositories
{
    internal class LoginRepository
    {
        public async Task<LoginResult> ValidateCredentialsAsync(
            string identifier, string password)
        {
            List<string> storedHashes;

            using (BackgammonEntities databaseContext = new BackgammonEntities())
            {
                storedHashes = await databaseContext.Accounts
                    .Where(account => account.Email == identifier ||
                        account.Profile.Username == identifier)
                    .Select(account => account.Password)
                    .ToListAsync();
            }

            if (storedHashes.Count == 0)
            {
                return LoginResult.UserNotFound;
            }

            PasswordHashService hashService = new PasswordHashService();
            bool passwordMatches = await Task.Run(() =>
                storedHashes.Any(hash =>
                    hashService.VerifyPassword(password, hash)));

            return passwordMatches
                ? LoginResult.Success
                : LoginResult.IncorrectPassword;
        }

    }
}
