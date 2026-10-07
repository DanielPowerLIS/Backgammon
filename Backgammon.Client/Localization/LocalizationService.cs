using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows.Data;
namespace Backgammon.Client.Localization
{
    public sealed class LocalizationService : INotifyPropertyChanged
    {
        private static readonly LocalizationService SharedInstance =
            new LocalizationService();

        private CultureInfo _currentCulture =
            CultureInfo.GetCultureInfo(LanguageOptions.EnglishCultureName);

        private LocalizationService()
        {
        }

        public static LocalizationService Current
        {
            get
            {
                return SharedInstance;
            }
        }

        public string CultureName
        {
            get
            {
                return _currentCulture.Name;
            }
        }

        public string this[string resourceKey]
        {
            get
            {
                return Properties.Resources.ResourceManager.GetString(
                    resourceKey,
                    _currentCulture) ?? resourceKey;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void LoadPreferredLanguage()
        {
            ApplyLanguage(Properties.Settings.Default.LanguageCultureName);
        }

        public void ChangeLanguage(string cultureName)
        {
            ApplyLanguage(cultureName);

            Properties.Settings.Default.LanguageCultureName = CultureName;
            Properties.Settings.Default.Save();
        }

        public void ApplyLanguage(string cultureName)
        {
            string supportedCultureName = GetSupportedCultureName(cultureName);

            _currentCulture = CultureInfo.GetCultureInfo(supportedCultureName);

            Thread.CurrentThread.CurrentCulture = _currentCulture;
            Thread.CurrentThread.CurrentUICulture = _currentCulture;

            CultureInfo.DefaultThreadCurrentCulture = _currentCulture;
            CultureInfo.DefaultThreadCurrentUICulture = _currentCulture;

            Properties.Resources.Culture = _currentCulture;

            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(Binding.IndexerName));

            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(nameof(CultureName)));
        }

        private string GetSupportedCultureName(string cultureName)
        {
            bool isSpanishCulture = string.Equals(
                cultureName,
                LanguageOptions.SpanishCultureName,
                StringComparison.OrdinalIgnoreCase);

            return isSpanishCulture
                ? LanguageOptions.SpanishCultureName
                : LanguageOptions.EnglishCultureName;
        }
    }
}
