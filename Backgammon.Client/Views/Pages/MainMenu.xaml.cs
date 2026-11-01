using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.ComponentModel;

using Backgammon.Client.Controllers.Session;
using Backgammon.Client.Settings.Logging;
using Backgammon.Client.Views.Dialog;

namespace Backgammon.Client.Views.Pages
{
    public partial class MainMenu : Page
    {
        private readonly LogOutController _logOutController;
        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(MainMenu));

        public MainMenu()
        {
            InitializeComponent();

            _logOutController = new LogOutController();

            Loaded += OnPageLoaded;
        }

        private void OnPlayClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Preinitiation());
        }

        private void OnLeaderboardClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Leaderboard());
        }

        private void OnProfileClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PlayerProfile());
        }

        private void OnFriendsClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Friends());
        }

        private void OnSettingsClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Settings());
        }

        private async void OnSignOutClick(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;

            try
            {
                await _logOutController.LogOutAsync();
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(communicationException,
                    "A communication failure prevented remote sign-out from being confirmed.");

                ShowLogOutConnectionError();
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(timeoutException,
                    "The sign-out request exceeded the time limit. Remote sign-out could not be confirmed.");

                ShowLogOutConnectionError();
            }
            finally
            {
                IsEnabled = true;
                NavigateHomeAfterLogOut();
            }
        }

        private void OnRulesClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Rules());
        }

        private void NavigateHomeAfterLogOut()
        {
            NavigationService.Navigated += OnLogOutNavigationCompleted;
            NavigationService.Navigate(new Home());
        }

        private void OnLogOutNavigationCompleted(object sender, NavigationEventArgs navigationArguments)
        {
            var navigationFrame = (Frame)sender;
            NavigationService pageNavigation = navigationFrame.NavigationService;

            pageNavigation.Navigated -= OnLogOutNavigationCompleted;

            while (pageNavigation.CanGoBack)
            {
                pageNavigation.RemoveBackEntry();
            }
        }

        private void ShowLogOutConnectionError()
        {
            var connectionDialog = new AccountDialog(
                Properties.Resources.TextBlock_LogOutConnectionErrorTitle,
                Properties.Resources.TextBlock_LogOutConnectionErrorMessage);

            connectionDialog.ShowOver(Window.GetWindow(this));
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            bool accountFeaturesEnabled = !PlayerSessionContext.GetSession().IsGuest;

            buttonLeaderboard.IsEnabled = accountFeaturesEnabled;
            buttonProfile.IsEnabled = accountFeaturesEnabled;
            buttonFriends.IsEnabled = accountFeaturesEnabled;
        }
    }
}
