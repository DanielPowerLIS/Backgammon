namespace Backgammon.Client.Models
{
    internal sealed class RegistrationValidationResult
    {
        public bool IsValid { get; set; }

        public string ErrorCode { get; set; } = string.Empty;

        public string FieldName { get; set; } = string.Empty;
    }
}
