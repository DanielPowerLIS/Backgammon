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
                ErrorCode = "RequiredFields",
                FieldName = fieldName
            };

            return validationResult;
        }
    }
}
