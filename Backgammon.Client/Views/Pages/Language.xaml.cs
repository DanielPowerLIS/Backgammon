using System.Windows;
using System.Windows.Controls;

using Backgammon.Client.Localization;

namespace Backgammon.Client.Views.Pages
{
    /// <summary>
    /// Lógica de interacción para Language.xaml
    /// </summary>
    public partial class Language : Page
    {
        public Language()
        {
            InitializeComponent();
        }

        private void OnSpanishClick(object sender, RoutedEventArgs e)
        {
            LocalizationService.Current.ChangeLanguage(
                LanguageOptions.SpanishCultureName);
        }

        private void OnEnglishClick(object sender, RoutedEventArgs e)
        {
            LocalizationService.Current.ChangeLanguage(
                LanguageOptions.EnglishCultureName);
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Settings());
        }
    }
}
