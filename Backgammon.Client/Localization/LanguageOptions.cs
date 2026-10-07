namespace Backgammon.Client.Localization
{
    public static class LanguageOptions
    {
        private static readonly string SpanishLanguageCulture = "es";
        private static readonly string EnglishLanguageCulture = "en-US";

        public static string SpanishCultureName
        {
            get
            {
                return SpanishLanguageCulture;
            }
        }

        public static string EnglishCultureName
        {
            get
            {
                return EnglishLanguageCulture;
            }
        }
    }
}
