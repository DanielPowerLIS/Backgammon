namespace Backgammon.Client.Controllers.Data
{
    public sealed class InputValidationResult
    {
        public bool IsValid { get; set; }

        public string ErrorCode { get; set; } = string.Empty;

        public string FieldName { get; set; } = string.Empty;
    }
}
