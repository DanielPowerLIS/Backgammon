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
                ApplicationLogger.LogError(guestException, "The server could not complete guest access. Code: {0}",
                    guestException.Detail.ErrorCode);

                ShowGuestAccessError();
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(communicationException,
                    "A communication failure prevented guest access from completing.");

                ShowGuestAccessError();
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(timeoutException, "The guest access request exceeded the time limit.");

                ShowGuestAccessError();
            }
        }

        private void ShowGuestAccessError()
        {
            var errorDialog = new AccountDialog(
                Properties.Resources.TextBlock_GuestAccessErrorTitle,
                Properties.Resources.TextBlock_GuestAccessErrorMessage);

            errorDialog.ShowOver(Window.GetWindow(this));
        }
    }
}
