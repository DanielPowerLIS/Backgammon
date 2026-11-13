using System.Threading.Tasks;

using Backgammon.Client.AuthenticationReference;
using Backgammon.Client.Comunication;
using Backgammon.Client.Controllers.Data;
using Backgammon.Client.Controllers.Session;

namespace Backgammon.Client.Controllers.Verification
{
    internal class VerificationController
    {
        private readonly AuthenticationClient _authenticationClient;

        private static readonly int CodeLength = 6;

        private static readonly string AllowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        private static readonly string InvalidCodeFormatErrorCode = "InvalidCodeFormat";

        public VerificationController()
        {
            _authenticationClient = new AuthenticationClient();
        }

        public async Task<InputValidationResult> ConfirmVerificationAsync(string requestId, string code)
        {
            InputValidationResult validationResult = ValidateCode(code);

            if (!validationResult.IsValid)
            {
                return validationResult;
            }

            PlayerSession playerSession = await _authenticationClient.ConfirmVerificationAsync(requestId, code);

            PlayerSessionContext.StoreSession(playerSession);

            return validationResult;
        }

        private InputValidationResult ValidateCode(string code)
        {
            bool hasValidLength = code != null && code.Length == CodeLength;
            bool isValid = false;

            if (hasValidLength)
            {
                isValid = HasOnlyAllowedCharacters(code);
            }

            var validationResult = new InputValidationResult
            {
                IsValid = isValid
            };

            if (!isValid)
            {
                validationResult.ErrorCode = InvalidCodeFormatErrorCode;
                validationResult.FieldName = nameof(code);
            }

            return validationResult;
        }

        private bool HasOnlyAllowedCharacters(string code)
        {
            foreach (char character in code)
            {
                string characterText = character.ToString();
                bool isAllowed = AllowedCharacters.Contains(characterText);

                if (!isAllowed)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
