using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;

using Backgammon.Client.Controllers.Session;

namespace Backgammon.Client.Views.Pages
{
    public partial class Settings : Page
    {
        public Settings()
        {
            InitializeComponent();

            Loaded += OnPageLoaded;
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MainMenu());
        }

        private void OnLanguageClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Language());
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            bool accountFeaturesEnabled =
                !PlayerSessionContext.GetSession().IsGuest;

            buttonAccount.IsEnabled = accountFeaturesEnabled;
            buttonChangePassword.IsEnabled = accountFeaturesEnabled;
        }
    }
}
