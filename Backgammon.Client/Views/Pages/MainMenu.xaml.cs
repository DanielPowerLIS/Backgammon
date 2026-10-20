using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using Backgammon.Client.Controllers.Session;
using Backgammon.Client.Settings.Logging;
using Backgammon.Client.Views.Dialog;

namespace Backgammon.Client.Views.Pages
{
    public partial class MainMenu : Page
    {
        private readonly LogOutController _logOutController;
        private readonly IApplicationLogger _applicationLogger;

        public MainMenu()
        {
            InitializeComponent();

            _logOutController = new LogOutController();
            _applicationLogger = new Log4NetApplicationLogger(typeof(MainMenu));
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
            catch (Exception connectionException) when (
                connectionException is CommunicationException ||
                connectionException is TimeoutException)
            {
                ShowLogOutConnectionError(connectionException);
            }
            finally
            {
                IsEnabled = true;
                NavigateHomeAfterLogOut();
            }
        }

        private void NavigateHomeAfterLogOut()
        {
            NavigationService.Navigated += OnLogOutNavigationCompleted;
            NavigationService.Navigate(new Home());
        }

        private void OnLogOutNavigationCompleted(
            object sender, NavigationEventArgs navigationArguments)
        {
            var navigationFrame = (Frame)sender;
            NavigationService pageNavigation = navigationFrame.NavigationService;

            pageNavigation.Navigated -= OnLogOutNavigationCompleted;

            while (pageNavigation.CanGoBack)
            {
                pageNavigation.RemoveBackEntry();
            }
        }

        private void ShowLogOutConnectionError(Exception errorException)
        {
            _applicationLogger.LogError(
                errorException,
                "Remote sign-out could not be confirmed.");

            var connectionDialog = new AccountDialog(
                Properties.Resources.TextBlock_LogOutConnectionErrorTitle,
                Properties.Resources.TextBlock_LogOutConnectionErrorMessage);

            connectionDialog.ShowOver(Window.GetWindow(this));
        }
    }
}
