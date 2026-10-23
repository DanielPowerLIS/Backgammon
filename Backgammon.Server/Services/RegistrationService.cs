using System.ServiceModel;
using System;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Security.Cryptography;

using Backgammon.Server.SupportServices.Logging;
using Backgammon.Server.Persistence.Repositories;
using Backgammon.Server.SupportServices;
using Backgammon.Server.Services.Data;
using AccountEntity = Backgammon.Server.Persistence.Data.Account;
using ProfileEntity = Backgammon.Server.Persistence.Data.Profile;
using Backgammon.Server.Services.Contracts;

namespace Backgammon.Server.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly AccountRepository _accountRepository;
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
        private static readonly int OfflineStateId = 3;

        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(RegistrationService));

        public RegistrationService()
        {
            _accountRepository = new AccountRepository();
            _passwordHashService = new PasswordHashService();
        }

        public async Task RegisterAccountAsync(Account accountInformation)
        {
            ValidateAccountInformation(accountInformation);

            try
            {
                await RegisterValidatedAccountAsync(accountInformation);
            }
            catch (Exception errorException) when (
                IsRegistrationFailure(errorException))
            {
                ApplicationLogger.LogError(
                    errorException,
                    "Account registration could not be completed.");

                throw CreateRegistrationFault(ServiceUnavailableErrorCode);
            }
        }

        private bool IsRegistrationFailure(Exception errorException)
        {
            bool registrationFailure =
                errorException is EntityException ||
                errorException is SqlException ||
                errorException is DbUpdateException ||
                errorException is DbEntityValidationException ||
                errorException is CryptographicException ||
                errorException is InvalidOperationException;

            return registrationFailure;
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
                IdAvailableStateFK = OfflineStateId
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

        private async Task ValidateAvailabilityAsync(Account accountInformation)
        {
            string emailAddress = accountInformation.Email.Trim();
            string playerUsername = accountInformation.Username.Trim();

            bool emailRegistered = await _accountRepository.IsEmailRegisteredAsync(emailAddress);

            if (emailRegistered)
            {
                throw CreateRegistrationFault(EmailAlreadyRegisteredErrorCode);
            }

            bool usernameRegistered = await _accountRepository.IsUsernameRegisteredAsync(playerUsername);

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
            catch (FormatException)
            {
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

        private async Task RegisterValidatedAccountAsync(Account accountInformation)
        {
            await ValidateAvailabilityAsync(accountInformation);

            AccountEntity playerAccount = CreateAccount(accountInformation);

            try
            {
                await _accountRepository.CreateAccountAsync(playerAccount);
            }
            catch (Exception errorException) when (
                IsUniqueViolation(errorException))
            {
                await ValidateAvailabilityAsync(accountInformation);

                ApplicationLogger.LogError(
                    errorException,
                    "Account registration encountered an unresolved uniqueness conflict.");

                throw CreateRegistrationFault(ServiceUnavailableErrorCode);
            }
        }
    }
}