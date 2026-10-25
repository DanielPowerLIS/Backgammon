using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Security.Cryptography;

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

            try
            {
                return AuthenticatePlayer(playerCredentials);
            }
            catch (Exception authenticationException) when (
                IsAuthenticationFailure(authenticationException))
            {
                ApplicationLogger.LogError(
                    authenticationException,
                    "A technical failure prevented sign-in from completing.");

                throw CreateAuthenticationFault(ServiceUnavailableErrorCode);
            }
        }

        private PlayerSession AuthenticatePlayer(Credentials playerCredentials)
        {

            string accountIdentifier = playerCredentials.Identifier.Trim();

            List<Account> matchingAccounts = FindAccounts(accountIdentifier);

            PlayerSession playerSession = CreateAuthenticatedSession(
                matchingAccounts, playerCredentials.Password);

            ApplicationLogger.LogInformation(
                "Player {0} signed in successfully.",
                playerSession.PlayerId);

            return playerSession;
        }

        private bool IsAuthenticationFailure(Exception authenticationException)
        {
            return authenticationException is EntityException ||
                authenticationException is SqlException ||
                authenticationException is CryptographicException ||
                authenticationException is InvalidOperationException;
        }

        private List<Account> FindAccounts(string accountIdentifier)
        {
            List<Account> matchingAccounts = _accountRepository
                .GetAccountsByIdentifierAsync(accountIdentifier)
                .GetAwaiter()
                .GetResult();

            return matchingAccounts;
        }

        private PlayerSession CreateAuthenticatedSession(List<Account> matchingAccounts, string suppliedPassword)
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
