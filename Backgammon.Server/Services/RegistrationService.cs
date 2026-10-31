using System;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.ServiceModel;

using Backgammon.Server.Persistence.Repositories;
using Backgammon.Server.Persistence.Repositories.InterfacesRepositories;
using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Services.Data;
using Backgammon.Server.SupportServices;
using Backgammon.Server.SupportServices.Logging;
using AccountEntity = Backgammon.Server.Persistence.Data.Account;
using ProfileEntity = Backgammon.Server.Persistence.Data.Profile;

namespace Backgammon.Server.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly PasswordHashService _passwordHashService;
        private static readonly string InvalidInputErrorCode = "InvalidInput";
        private static readonly string EmailAlreadyRegisteredErrorCode = "EmailAlreadyRegistered";
        private static readonly string UsernameAlreadyTakenErrorCode = "UsernameAlreadyTaken";
        private static readonly string ServiceUnavailableErrorCode = "ServiceUnavailable";
        private static readonly int DuplicateIndexErrorNumber = 2601;
        private static readonly int DuplicateConstraintErrorNumber = 2627;
        private static readonly int MaximumTextFieldLength = 100;
        private static readonly int MinimumPasswordLength = 8;
        private static readonly int DefaultAvatarId = 1;

        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(RegistrationService));

        public RegistrationService()
        {
            _accountRepository = new AccountRepository();
            _passwordHashService = new PasswordHashService();
        }

        public void RegisterAccount(Account accountInformation)
        {
            ValidateAccountInformation(accountInformation);

            try
            {
                RegisterValidatedAccount(accountInformation);
            }
            catch (EntityException entityException)
            {
                ApplicationLogger.LogError(entityException,
                    "An Entity Framework failure prevented account registration.");

                throw CreateRegistrationFault(ServiceUnavailableErrorCode);
            }
            catch (SqlException databaseException)
            {
                ApplicationLogger.LogError(databaseException,
                    "A SQL Server failure prevented account registration.");

                throw CreateRegistrationFault(ServiceUnavailableErrorCode);
            }
            catch (DbUpdateException updateException)
            {
                ApplicationLogger.LogError(updateException,
                    "The account and profile could not be saved to the database.");

                throw CreateRegistrationFault(ServiceUnavailableErrorCode);
            }
            catch (DbEntityValidationException validationException)
            {
                ApplicationLogger.LogError(validationException,
                    "Entity validation failed while saving the account and profile.");

                throw CreateRegistrationFault(ServiceUnavailableErrorCode);
            }
            catch (CryptographicException cryptographicException)
            {
                ApplicationLogger.LogError(cryptographicException,
                    "A cryptographic failure prevented account registration.");

                throw CreateRegistrationFault(ServiceUnavailableErrorCode);
            }
            catch (InvalidOperationException operationException)
            {
                ApplicationLogger.LogError(operationException,
                    "An invalid operation prevented account registration.");

                throw CreateRegistrationFault(ServiceUnavailableErrorCode);
            }
        }

        private AccountEntity CreateAccount(Account accountInformation)
        {
            string passwordHash = _passwordHashService.HashPassword(accountInformation.Password);

            ProfileEntity playerProfile = CreateProfile(accountInformation.Username.Trim());

            var playerAccount = new AccountEntity
            {
                Email = accountInformation.Email.Trim(),
                Password = passwordHash,
                RegistrationDate = DateTime.UtcNow,
                Name = accountInformation.Name.Trim(),
                PaternalSurname = accountInformation.PaternalSurname.Trim(),
                MaternalSurname = accountInformation.MaternalSurname.Trim(),
                Profile = playerProfile
            };

            return playerAccount;
        }

        private ProfileEntity CreateProfile(string playerUsername)
        {
            var playerProfile = new ProfileEntity
            {
                Username = playerUsername,
                Wins = 0,
                Points = 0,
                IdAvatarFK = DefaultAvatarId,
            };

            return playerProfile;
        }

        private void ValidateAccountInformation(Account accountInformation)
        {
            ValidateRequiredFields(accountInformation);
            ValidateFieldLengths(accountInformation);
            ValidateUsernameFormat(accountInformation.Username);

            string emailAddress = accountInformation.Email.Trim();

            if (!HasValidEmailFormat(emailAddress))
            {
                throw CreateRegistrationFault(InvalidInputErrorCode);
            }

            ValidatePasswordFormat(accountInformation.Password);
        }

        private void ValidateUsernameFormat(string playerUsername)
        {
            if (playerUsername.Contains("@"))
            {
                throw CreateRegistrationFault(InvalidInputErrorCode);
            }
        }

        private void ValidateAvailability(Account accountInformation)
        {
            string emailAddress = accountInformation.Email.Trim();
            string playerUsername = accountInformation.Username.Trim();

            bool emailRegistered = _accountRepository
                .IsEmailRegisteredAsync(emailAddress).GetAwaiter().GetResult();

            if (emailRegistered)
            {
                throw CreateRegistrationFault(EmailAlreadyRegisteredErrorCode);
            }

            bool usernameRegistered = _accountRepository
                .IsUsernameRegisteredAsync(playerUsername).GetAwaiter().GetResult();

            if (usernameRegistered)
            {
                throw CreateRegistrationFault(UsernameAlreadyTakenErrorCode);
            }
        }

        private void ValidateRequiredFields(Account accountInformation)
        {
            if (accountInformation == null ||
                string.IsNullOrWhiteSpace(accountInformation.Email) ||
                string.IsNullOrWhiteSpace(accountInformation.Password) ||
                string.IsNullOrWhiteSpace(accountInformation.Username) ||
                string.IsNullOrWhiteSpace(accountInformation.Name) ||
                string.IsNullOrWhiteSpace(accountInformation.PaternalSurname) ||
                string.IsNullOrWhiteSpace(accountInformation.MaternalSurname))
            {
                throw CreateRegistrationFault(InvalidInputErrorCode);
            }
        }

        private void ValidateFieldLengths(Account accountInformation)
        {
            string[] registrationFields =
            {
                accountInformation.Email,
                accountInformation.Username,
                accountInformation.Name,
                accountInformation.PaternalSurname,
                accountInformation.MaternalSurname
            };

            foreach (string registrationField in registrationFields)
            {
                if (registrationField.Trim().Length > MaximumTextFieldLength)
                {
                    throw CreateRegistrationFault(InvalidInputErrorCode);
                }
            }
        }

        private bool HasValidEmailFormat(string emailAddress)
        {
            if (string.IsNullOrWhiteSpace(emailAddress) ||
                emailAddress.Any(char.IsWhiteSpace))
            {
                return false;
            }

            try
            {
                var parsedAddress = new MailAddress(emailAddress);

                bool validEmailFormat =
                    string.Equals(parsedAddress.Address, emailAddress, StringComparison.Ordinal) &&
                    Uri.CheckHostName(parsedAddress.Host) == UriHostNameType.Dns &&
                    !parsedAddress.Host.Contains("_") &&
                    parsedAddress.Host.Contains(".") &&
                    !parsedAddress.Host.EndsWith(".", StringComparison.Ordinal) &&
                    !parsedAddress.Host.Contains("..");

                return validEmailFormat;
            }
            catch (FormatException formatException)
            {
                ApplicationLogger.LogWarning(
                    "Email address validation failed due to an invalid format. Reason: {0}",
                    formatException.Message);

                return false;
            }
        }

        private void ValidatePasswordFormat(string suppliedPassword)
        {
            bool hasMinimumLength =
                suppliedPassword.Length >= MinimumPasswordLength;

            bool hasSpecialCharacter = suppliedPassword.Any(
                passwordCharacter => char.IsPunctuation(passwordCharacter) ||
                    char.IsSymbol(passwordCharacter));

            if (!hasMinimumLength || !hasSpecialCharacter)
            {
                throw CreateRegistrationFault(InvalidInputErrorCode);
            }
        }

        private FaultException<RegistrationFault> CreateRegistrationFault(
            string errorCode)
        {
            var registrationFault = new RegistrationFault
            {
                ErrorCode = errorCode
            };

            var faultException =
                new FaultException<RegistrationFault>(registrationFault);

            return faultException;
        }

        private bool IsUniqueViolation(Exception errorException)
        {
            for (Exception currentException = errorException;
                currentException != null;
                currentException = currentException.InnerException)
            {
                if (currentException is SqlException databaseException)
                {
                    bool duplicateDetected = databaseException.Errors
                        .Cast<SqlError>()
                        .Any(databaseError =>
                            databaseError.Number == DuplicateIndexErrorNumber ||
                            databaseError.Number == DuplicateConstraintErrorNumber);

                    if (duplicateDetected)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void RegisterValidatedAccount(Account accountInformation)
        {
            ValidateAvailability(accountInformation);

            AccountEntity playerAccount = CreateAccount(accountInformation);

            try
            {
                _accountRepository.CreateAccountAsync(playerAccount)
                    .GetAwaiter().GetResult();
            }
            catch (DbUpdateException updateException)
            {
                bool duplicateDetected = IsUniqueViolation(updateException);

                if (!duplicateDetected)
                {
                    throw;
                }

                ApplicationLogger.LogError(updateException,
                    "Account registration encountered an unresolved uniqueness conflict.");

                ValidateAvailability(accountInformation);

                throw CreateRegistrationFault(ServiceUnavailableErrorCode);
            }
            catch (SqlException databaseException)
            {
                bool duplicateDetected = IsUniqueViolation(databaseException);

                if (!duplicateDetected)
                {
                    throw;
                }

                ApplicationLogger.LogError(databaseException,
                    "Account registration encountered an unresolved SQL uniqueness conflict.");

                ValidateAvailability(accountInformation);

                throw CreateRegistrationFault(ServiceUnavailableErrorCode);
            }
        }
    }
}