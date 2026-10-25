using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Net.Mail;

using Backgammon.Client.Controllers.Data;
using Backgammon.Client.Comunication;
using Backgammon.Client.RegistrationReference;

namespace Backgammon.Client.Controllers.Registration
{
    internal class RegistrationController
    {
        private readonly RegistrationClient _registrationClient;

        private static readonly string RequiredFieldsErrorCode = "RequiredFields";
        private static readonly string InvalidFieldLengthErrorCode = "InvalidFieldLength";
        private static readonly string InvalidUsernameErrorCode = "InvalidUsername";
        private static readonly string InvalidPasswordErrorCode = "InvalidPassword";
        private static readonly string InvalidEmailErrorCode = "InvalidEmail";

        private static readonly int MinimumPasswordLength = 8;

        public RegistrationController()
        {
            _registrationClient = new RegistrationClient();
        }

        public async Task<InputValidationResult> RegisterAccountAsync(Account accountInformation)
        {
            InputValidationResult validationResult =
                ValidateLocalFields(accountInformation);

            if (!validationResult.IsValid)
            {
                return validationResult;
            }

            await _registrationClient.RegisterAccountAsync(accountInformation);

            return validationResult;
        }

        private InputValidationResult ValidateLocalFields(Account accountInformation)
        {
            InputValidationResult validationResult =
                ValidateRequiredFields(accountInformation);

            if (!validationResult.IsValid)
            {
                return validationResult;
            }

            validationResult = ValidateFieldLengths(accountInformation);

            if (!validationResult.IsValid)
            {
                return validationResult;
            }

            return ValidateFieldFormats(accountInformation);
        }

        private InputValidationResult ValidateFieldFormats(Account accountInformation)
        {
            InputValidationResult validationResult =
                ValidateEmailFormat(accountInformation.Email);

            if (!validationResult.IsValid)
            {
                return validationResult;
            }

            validationResult = ValidateUsernameFormat(accountInformation.Username);

            if (!validationResult.IsValid)
            {
                return validationResult;
            }

            return ValidatePasswordFormat(accountInformation.Password);
        }

        private KeyValuePair<string, string>[] GetRequiredFields(Account accountInformation)
        {
            var requiredFields = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>(nameof(Account.Email), accountInformation.Email),
                new KeyValuePair<string, string>(nameof(Account.Name), accountInformation.Name),
                new KeyValuePair<string, string>(nameof(Account.PaternalSurname), accountInformation.PaternalSurname),
                new KeyValuePair<string, string>(nameof(Account.MaternalSurname), accountInformation.MaternalSurname),
                new KeyValuePair<string, string>(nameof(Account.Username), accountInformation.Username),
                new KeyValuePair<string, string>(nameof(Account.Password), accountInformation.Password)
            };

            return requiredFields;
        }

        private InputValidationResult ValidateRequiredFields(Account accountInformation)
        {
            if (accountInformation == null)
            {
                return CreateValidationError(RequiredFieldsErrorCode, string.Empty);
            }

            foreach (KeyValuePair<string, string> requiredField in
                GetRequiredFields(accountInformation))
            {
                if (string.IsNullOrWhiteSpace(requiredField.Value))
                {
                    return CreateValidationError(
                        RequiredFieldsErrorCode, requiredField.Key);
                }
            }

            var validationResult = new InputValidationResult { IsValid = true };

            return validationResult;
        }

        private InputValidationResult CreateValidationError(string errorCode, string fieldName)
        {
            var validationResult = new InputValidationResult
            {
                IsValid = false,
                ErrorCode = errorCode,
                FieldName = fieldName
            };

            return validationResult;
        }

        private InputValidationResult ValidateFieldLengths(Account accountInformation)
        {
            foreach (KeyValuePair<string, string> registrationField in
                GetRequiredFields(accountInformation))
            {
                if (registrationField.Key != nameof(Account.Password) &&
                    registrationField.Value.Trim().Length >
                        RegistrationLimits.TextFieldMaximumLength)
                {
                    return CreateValidationError(
                        InvalidFieldLengthErrorCode, registrationField.Key);
                }
            }

            var validationResult = new InputValidationResult 
            {
                IsValid = true 
            };

            return validationResult;
        }

        private InputValidationResult ValidateUsernameFormat(string playerUsername)
        {
            if (playerUsername.Contains("@"))
            {
                return CreateValidationError(
                    InvalidUsernameErrorCode, nameof(Account.Username));
            }

            var validationResult = new InputValidationResult 
            { 
                IsValid = true 
            };

            return validationResult;
        }

        private InputValidationResult ValidatePasswordFormat(string suppliedPassword)
        {
            bool hasMinimumLength = suppliedPassword.Length >= MinimumPasswordLength;
            bool hasSpecialCharacter = suppliedPassword.Any(
                passwordCharacter => char.IsPunctuation(passwordCharacter) ||
                    char.IsSymbol(passwordCharacter));

            if (!hasMinimumLength || !hasSpecialCharacter)
            {
                return CreateValidationError(
                    InvalidPasswordErrorCode, nameof(Account.Password));
            }

            var validationResult = new InputValidationResult
            {
                IsValid = true
            };

            return validationResult;
        }

        private InputValidationResult ValidateEmailFormat(string emailAddress)
        {
            if (!HasValidEmailFormat(emailAddress.Trim()))
            {
                return CreateValidationError(
                    InvalidEmailErrorCode, nameof(Account.Email));
            }

            var validationResult = new InputValidationResult
            {
                IsValid = true
            };

            return validationResult;
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
                bool validEmailFormat = string.Equals(
                    parsedAddress.Address, emailAddress, StringComparison.Ordinal) &&
                    HasValidEmailHost(parsedAddress.Host);

                return validEmailFormat;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private bool HasValidEmailHost(string emailHost)
        {
            bool validEmailHost =
                Uri.CheckHostName(emailHost) == UriHostNameType.Dns &&
                !emailHost.Contains("_") &&
                emailHost.Contains(".") &&
                !emailHost.EndsWith(".", StringComparison.Ordinal) &&
                !emailHost.Contains("..");

            return validEmailHost;
        }
    }
}