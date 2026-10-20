using System.Threading.Tasks;
using System;

using Backgammon.Client.Models;
using Backgammon.Client.AuthenticationReference;
using Backgammon.Client.Comunication;
using Backgammon.Client.Controllers.Session;

namespace Backgammon.Client.Controllers.Authentication
{
    internal class AuthenticationController
    {
        private readonly AuthenticationClient _authenticationClient;

        private static readonly string RequiredFieldsErrorCode = "RequiredFields";

        public AuthenticationController()
        {
            _authenticationClient = new AuthenticationClient();
        }

        public async Task<InputValidationResult> SignInAsync(string accountIdentifier, string suppliedPassword)
        {
            InputValidationResult validationResult =
                ValidateRequiredCredentials(accountIdentifier, suppliedPassword);

            if (!validationResult.IsValid)
            {
                return validationResult;
            }

            var playerCredentials = new Credentials
            {
                Identifier = accountIdentifier.Trim(),
                Password = suppliedPassword
            };

            PlayerSession playerSession = await _authenticationClient.SignInAsync(playerCredentials);

            PlayerSessionContext.StoreSession(playerSession);

            return validationResult;
        }

        private InputValidationResult ValidateRequiredCredentials(string accountIdentifier, string suppliedPassword)
        {
            if (string.IsNullOrWhiteSpace(accountIdentifier))
            {
                return CreateRequiredFieldError(nameof(Credentials.Identifier));
            }

            if (string.IsNullOrWhiteSpace(suppliedPassword))
            {
                return CreateRequiredFieldError(nameof(Credentials.Password));
            }

            var validationResult = new InputValidationResult
            {
                IsValid = true
            };

            return validationResult;
        }

        private InputValidationResult CreateRequiredFieldError(string fieldName)
        {
            var validationResult = new InputValidationResult
            {
                IsValid = false,
                ErrorCode = RequiredFieldsErrorCode,
                FieldName = fieldName
            };

            return validationResult;
        }
    }
}
