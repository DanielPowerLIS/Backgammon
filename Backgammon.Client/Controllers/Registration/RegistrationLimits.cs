namespace Backgammon.Client.Controllers.Registration
{
    public static class RegistrationLimits
    {
        private static readonly int MaximumTextFieldLength = 100;

        public static int TextFieldMaximumLength
        {
            get
            {
                return MaximumTextFieldLength;
            }
        }
    }
}
