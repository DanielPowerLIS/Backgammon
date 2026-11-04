using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Configuration;
using System.IO;
using System.Net.Sockets;

using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

using Backgammon.Server.Persistence.Repositories;
using Backgammon.Server.Persistence.Repositories.InterfacesRepositories;
using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Services.Data;
using Backgammon.Server.SupportServices;
using Backgammon.Server.SupportServices.Logging;
using Account = Backgammon.Server.Persistence.Data.Account;

namespace Backgammon.Server.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly PasswordHashService _passwordHashService;
        private readonly PlayerSessionManager _playerSessionManager;
        private readonly CodeGenerator _codeGenerator;
        private readonly VerificationManager _verificationManager;

        private static readonly string InvalidInputErrorCode = "InvalidInput";
        private static readonly string InvalidCredentialsErrorCode = "InvalidCredentials";
        private static readonly string ServiceUnavailableErrorCode = "ServiceUnavailable";

        private static readonly int VerificationCodeLength = 6;

        private static readonly string VerificationEmailSubject = "Backgammon verification code";

        private static readonly string VerificationEmailBody = "Your verification code is: {0}";

        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(AuthenticationService));

        public AuthenticationService()
        {
            _accountRepository = new AccountRepository();
            _passwordHashService = new PasswordHashService();
            _playerSessionManager = new PlayerSessionManager();
            _codeGenerator = new CodeGenerator();
            _verificationManager = new VerificationManager();
        }

        public VerificationRequest SignIn(Credentials playerCredentials)
        {
            ValidateCredentials(playerCredentials);

            try
            {
                VerificationRequest request = AuthenticatePlayer(playerCredentials);

                return request;
            }
            catch (EntityException entityException)
            {
                ApplicationLogger.LogError(entityException,
                    "An Entity Framework failure prevented sign-in from completing.");

                throw CreateAuthenticationFault(ServiceUnavailableErrorCode);
            }
            catch (SqlException databaseException)
            {
                ApplicationLogger.LogError(databaseException,
                    "A SQL Server failure prevented sign-in from completing.");

                throw CreateAuthenticationFault(ServiceUnavailableErrorCode);
            }
            catch (CryptographicException cryptographicException)
            {
                ApplicationLogger.LogError(cryptographicException,
                    "A cryptographic failure prevented sign-in from completing.");

                throw CreateAuthenticationFault(ServiceUnavailableErrorCode);
            }
        }

        public PlayerSession ConfirmVerification(string requestId, string code)
        {
            try
            {
                int accountId = _verificationManager.VerifyRequest(requestId, code);

                PlayerSession playerSession = CreateAuthenticatedSession(accountId);

                ApplicationLogger.LogInformation("Player {0} signed in after completing verification.",
                    playerSession.PlayerId);

                return playerSession;
            }
            catch (EntityException entityException)
            {
                ApplicationLogger.LogError(entityException,
                    "An Entity Framework failure prevented verified sign-in from completing.");

                throw CreateAuthenticationFault(ServiceUnavailableErrorCode);
            }
            catch (SqlException databaseException)
            {
                ApplicationLogger.LogError(databaseException,
                    "A SQL Server failure prevented verified sign-in from completing.");

                throw CreateAuthenticationFault(ServiceUnavailableErrorCode);
            }
            catch (CryptographicException cryptographicException)
            {
                ApplicationLogger.LogError(cryptographicException,
                    "A cryptographic failure prevented verified sign-in from completing.");

                throw CreateAuthenticationFault(ServiceUnavailableErrorCode);
            }
            catch (KeyNotFoundException accountException)
            {
                ApplicationLogger.LogWarning(
                    "Verified sign-in was rejected because the account no longer exists. Reason: {0}",
                    accountException.Message);

                throw CreateAuthenticationFault(InvalidCredentialsErrorCode);
            }
        }

        private PlayerSession CreateAuthenticatedSession(int accountId)
        {
            Account account = _accountRepository .GetAccountByIdAsync(accountId).GetAwaiter().GetResult();

            if (account.Profile == null)
            {
                ApplicationLogger.LogWarning(
                    "Verified sign-in was rejected because account {0} has no player profile.",
                    accountId);

                throw CreateAuthenticationFault(InvalidCredentialsErrorCode);
            }

            PlayerSession playerSession = _playerSessionManager.CreateSession(account.Profile.IdProfilePK,
                account.Profile.Username);

            return playerSession;
        }

        private VerificationRequest AuthenticatePlayer(Credentials playerCredentials)
        {
            string accountIdentifier = playerCredentials.Identifier.Trim();

            List<Account> matchingAccounts = FindAccounts(accountIdentifier);

            Account authenticatedAccount = GetAuthenticatedAccount(matchingAccounts, playerCredentials.Password);

            VerificationRequest request = CreateVerificationRequest(authenticatedAccount);

            ApplicationLogger.LogInformation("A verification request was created for account {0}.",
                authenticatedAccount.IdAccountPK);

            return request;
        }

        private List<Account> FindAccounts(string accountIdentifier)
        {
            List<Account> matchingAccounts = _accountRepository
                .GetAccountsByIdentifierAsync(accountIdentifier)
                .GetAwaiter()
                .GetResult();

            return matchingAccounts;
        }

        private Account GetAuthenticatedAccount(List<Account> matchingAccounts, string suppliedPassword)
        {
            Account[] authenticatedAccounts = matchingAccounts
                .Where(playerAccount => playerAccount.Profile != null &&
                    _passwordHashService.VerifyPassword(suppliedPassword, playerAccount.Password)).ToArray();

            if (authenticatedAccounts.Length != 1)
            {
                ApplicationLogger.LogWarning("Sign-in rejected. Invalid credentials.");

                throw CreateAuthenticationFault(InvalidCredentialsErrorCode);
            }

            Account authenticatedAccount = authenticatedAccounts[0];

            return authenticatedAccount;
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

        private VerificationRequest CreateVerificationRequest(Account account)
        {
            string code = _codeGenerator.Generate(VerificationCodeLength);

            SendVerificationCode(account.Email, code);

            VerificationRequest request = _verificationManager.CreateRequest(account.IdAccountPK, code);

            return request;
        }

        private void SendVerificationCode(string recipient, string code)
        {
            try
            {
                string body = string.Format(VerificationEmailBody, code);

                var emailService = new EmailService();

                emailService.Send(recipient, VerificationEmailSubject, body);
            }
            catch (ConfigurationErrorsException configurationException)
            {
                ApplicationLogger.LogError(configurationException,
                    "The email configuration prevented the verification message from being sent.");

                throw CreateAuthenticationFault(ServiceUnavailableErrorCode);
            }
            catch (ParseException addressException)
            {
                ApplicationLogger.LogError(addressException,
                    "The recipient address could not be parsed.");

                throw CreateAuthenticationFault(ServiceUnavailableErrorCode);
            }
            catch (SocketException socketException)
            {
                ApplicationLogger.LogError(socketException,
                    "A network failure prevented connection to the email server.");

                throw CreateAuthenticationFault(ServiceUnavailableErrorCode);
            }
            catch (SslHandshakeException handshakeException)
            {
                ApplicationLogger.LogError(handshakeException,
                    "TLS negotiation with the email server failed.");

                throw CreateAuthenticationFault(ServiceUnavailableErrorCode);
            }
            catch (SaslException saslException)
            {
                ApplicationLogger.LogError(saslException,
                    "A SASL failure prevented authentication with the email server.");

                throw CreateAuthenticationFault(ServiceUnavailableErrorCode);
            }
            catch (MailKit.Security.AuthenticationException authenticationException)
            {
                ApplicationLogger.LogError(authenticationException,
                    "The email server rejected the sender credentials.");

                throw CreateAuthenticationFault(ServiceUnavailableErrorCode);
            }
            catch (SmtpCommandException commandException)
            {
                ApplicationLogger.LogError(commandException,
                    "An SMTP command failed while sending the verification message.");

                throw CreateAuthenticationFault(ServiceUnavailableErrorCode);
            }
            catch (SmtpProtocolException protocolException)
            {
                ApplicationLogger.LogError(protocolException,
                    "An SMTP protocol failure prevented the verification message from being sent.");

                throw CreateAuthenticationFault(ServiceUnavailableErrorCode);
            }
            catch (IOException communicationException)
            {
                ApplicationLogger.LogError(communicationException,
                    "An input/output failure occurred while communicating with the email server.");

                throw CreateAuthenticationFault(ServiceUnavailableErrorCode);
            }
            catch (NotSupportedException securityException)
            {
                ApplicationLogger.LogError(securityException,
                    "The email server does not support the required STARTTLS connection.");

                throw CreateAuthenticationFault(ServiceUnavailableErrorCode);
            }
        }

        private FaultException<AuthenticationFault> CreateAuthenticationFault(
            string errorCode)
        {
            var authenticationFault = new AuthenticationFault
            {
                ErrorCode = errorCode
            };

            var faultException = new FaultException<AuthenticationFault>(authenticationFault);

            return faultException;
        }
    }
}
