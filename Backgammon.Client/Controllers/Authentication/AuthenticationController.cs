using System.Threading.Tasks;

using Backgammon.Client.Controllers.Data;
using Backgammon.Client.AuthenticationReference;
using Backgammon.Client.Comunication;

namespace Backgammon.Client.Controllers.Authentication
{
    internal class AuthenticationController
    {
        private readonly AuthenticationClient _authenticationClient;

        public AuthenticationController()
        {
            _authenticationClient = new AuthenticationClient();
        }

        public string RequestId { get; private set; }

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

            VerificationRequest request = await _authenticationClient.SignInAsync(playerCredentials);

            RequestId = request.RequestId;

            return validationResult;
        }

        private InputValidationResult ValidateRequiredCredentials(string accountIdentifier, string suppliedPassword)
        {
            InputValidationResult validationResult;
            if (string.IsNullOrWhiteSpace(accountIdentifier))
            {
                validationResult = CreateRequiredFieldError(nameof(Credentials.Identifier));
                return validationResult;
            }

            if (string.IsNullOrWhiteSpace(suppliedPassword))
            {
                validationResult = CreateRequiredFieldError(nameof(Credentials.Password));
                return validationResult;
            }

            validationResult = new InputValidationResult
            {
                IsValid = true,
                ErrorCode = null,
                FieldName = null
            };

            return validationResult;
        }

        private InputValidationResult CreateRequiredFieldError(string fieldName)
        {
            var validationResult = new InputValidationResult
            {
                IsValid = false,
                ErrorCode = "RequiredFields",
                FieldName = fieldName
            };

            return validationResult;
        }
    }
}
