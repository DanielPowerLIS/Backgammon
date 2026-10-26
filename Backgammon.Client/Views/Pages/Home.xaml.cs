using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using Backgammon.Client.Controllers.GuestAccess;
using Backgammon.Client.GuestAccessReference;
using Backgammon.Client.Settings.Logging;
using Backgammon.Client.Views.Dialog;

namespace Backgammon.Client.Views.Pages
{
    public partial class Home : Page
    {
        private readonly GuestAccessController _guestAccessController;

        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(Home));

        public Home()
        {
            InitializeComponent();

            _guestAccessController = new GuestAccessController();
        }

        private void OnButtonSignIn(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Login());
        }

        private async void OnGuestClick(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;

            try
            {
                await AccessAsGuestWithFeedbackAsync();
            }
            finally
            {
                IsEnabled = true;
            }
        }

        private async Task AccessAsGuestWithFeedbackAsync()
        {
            try
            {
                await _guestAccessController.AccessAsGuestAsync();

                IsEnabled = true;

                NavigationService.Navigate(new MainMenu());
            }
            catch (FaultException<GuestAccessFault> guestException)
            {
                ShowGuestAccessError(guestException);
            }
            catch (Exception connectionException) when (
                connectionException is CommunicationException ||
                connectionException is TimeoutException)
            {
                ShowGuestAccessError(connectionException);
            }
        }

        private void ShowGuestAccessError(Exception errorException)
        {
            ApplicationLogger.LogError(
                errorException, "Guest access request failed.");

            var errorDialog = new AccountDialog(
                Properties.Resources.TextBlock_GuestAccessErrorTitle,
                Properties.Resources.TextBlock_GuestAccessErrorMessage);

            errorDialog.ShowOver(Window.GetWindow(this));
        }
    }
}
