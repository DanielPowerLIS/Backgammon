using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

using Backgammon.Server.Persistence.Data;
using Backgammon.Server.Persistence.Repositories;
using Backgammon.Server.Services.Data;
using Backgammon.Server.SupportServices;

namespace Backgammon.Server.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly AccountRepository _accountRepository;
        private readonly PasswordHashService _passwordHashService;
        private readonly PlayerSessionService _playerSessionService;

        private static readonly string InvalidInputErrorCode = "InvalidInput";

        private static readonly string InvalidCredentialsErrorCode = "InvalidCredentials";

        public AuthenticationService()
        {
            _accountRepository = new AccountRepository();
            _passwordHashService = new PasswordHashService();
            _playerSessionService = new PlayerSessionService();
        }

        public async Task<PlayerSession> SignInAsync(Credentials playerCredentials)
        {
            ValidateCredentials(playerCredentials);

            string accountIdentifier = playerCredentials.Identifier.Trim();

            List<Account> matchingAccounts =
                await _accountRepository.GetAccountsByIdentifierAsync(accountIdentifier);

            PlayerSession playerSession = CreateAuthenticatedSession(
                matchingAccounts, playerCredentials.Password);

            return playerSession;
        }

        private PlayerSession CreateAuthenticatedSession( List<Account> matchingAccounts, string suppliedPassword)
        {
            Account[] authenticatedAccounts = matchingAccounts
                .Where(playerAccount => playerAccount.Profile != null &&
                    _passwordHashService.VerifyPassword(
                        suppliedPassword, playerAccount.Password))
                .ToArray();

            if (authenticatedAccounts.Length != 1)
            {
                throw CreateAuthenticationFault(InvalidCredentialsErrorCode);
            }

            Account authenticatedAccount = authenticatedAccounts[0];

            PlayerSession playerSession = _playerSessionService.CreateSession(
                authenticatedAccount.Profile.IdProfilePK,
                authenticatedAccount.Profile.Username);

            return playerSession;
        }

        private void ValidateCredentials(Credentials playerCredentials)
        {
            if (playerCredentials == null ||
                string.IsNullOrWhiteSpace(playerCredentials.Identifier) ||
                string.IsNullOrWhiteSpace(playerCredentials.Password))
            {
                throw CreateAuthenticationFault(InvalidInputErrorCode);
            }
        }

        private FaultException<AuthenticationFault> CreateAuthenticationFault(
            string errorCode)
        {
            var authenticationFault = new AuthenticationFault
            {
                ErrorCode = errorCode
            };

            var faultException =
                new FaultException<AuthenticationFault>(authenticationFault);

            return faultException;
        }
    }
}
