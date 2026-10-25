using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System;
using System.Data.Entity.Core;
using System.Data.SqlClient;

using Account = Backgammon.Server.Persistence.Data.Account;
using Backgammon.Server.Persistence.Repositories;
using Backgammon.Server.Services.Data;
using Backgammon.Server.SupportServices;
using Backgammon.Server.SupportServices.Logging;
using Backgammon.Server.Services.Contracts;

namespace Backgammon.Server.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly AccountRepository _accountRepository;
        private readonly PasswordHashService _passwordHashService;
        private readonly PlayerSessionService _playerSessionService;
        private static readonly string InvalidInputErrorCode = "InvalidInput";
        private static readonly string InvalidCredentialsErrorCode = "InvalidCredentials";
        private static readonly string ServiceUnavailableErrorCode = "ServiceUnavailable";

        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(AuthenticationService));

        public AuthenticationService()
        {
            _accountRepository = new AccountRepository();
            _passwordHashService = new PasswordHashService();
            _playerSessionService = new PlayerSessionService();
        }

        public PlayerSession SignIn(Credentials playerCredentials)
        {
            ValidateCredentials(playerCredentials);

            string accountIdentifier = playerCredentials.Identifier.Trim();

            List<Account> matchingAccounts = FindAccounts(accountIdentifier);

            PlayerSession playerSession = CreateAuthenticatedSession(
                matchingAccounts, playerCredentials.Password);

            ApplicationLogger.LogInformation("Player {0} signed in successfully.");

            return playerSession;
        }

        private List<Account> FindAccounts(string accountIdentifier)
        {
            try
            {
                List<Account> matchingAccounts = _accountRepository
                    .GetAccountsByIdentifierAsync(accountIdentifier)
                    .GetAwaiter()
                    .GetResult();

                return matchingAccounts;
            }
            catch (Exception errorException) when (
                errorException is EntityException ||
                errorException is SqlException)
            {
                ApplicationLogger.LogError(
                    errorException,
                    "Database query failed during sign-in.");

                throw CreateAuthenticationFault(ServiceUnavailableErrorCode);
            }
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
                ApplicationLogger.LogWarning("Sign-in rejected. Invalid credentials.");

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
