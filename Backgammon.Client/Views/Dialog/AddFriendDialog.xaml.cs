using Backgammon.Client.Controllers.Friends;
using Backgammon.Client.FriendReference;
using Backgammon.Client.Settings.Logging;
using Backgammon.Client.Views.Models;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;

namespace Backgammon.Client.Views.Dialog
{
    public partial class AddFriendDialog : Window
    {
        private readonly FriendController _friendController = new FriendController();

        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(AddFriendDialog));

        public AddFriendDialog()
        {
            InitializeComponent();
        }

        public bool ShowFor(Window owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            Owner = owner;
            return ShowDialog() == true;
        }

        private async void OnAddByEmailClick(object sender, RoutedEventArgs e)
        {
            await SendRequestAsync(true);
        }

        private async void OnAddByCodeClick(object sender, RoutedEventArgs e)
        {
            await SendRequestAsync(false);
        }

        private async Task SendRequestAsync(bool byEmail)
        {
            string identifier = textBoxSearchValue.Text.Trim();

            if (string.IsNullOrWhiteSpace(identifier))
            {
                textBlockRequiredMessage.Visibility = Visibility.Visible;
                return;
            }

            textBlockRequiredMessage.Visibility = Visibility.Hidden;
            buttonAddByEmail.IsEnabled = false;
            buttonAddByCode.IsEnabled = false;

            try
            {
                await _friendController.SendFriendRequestAsync(identifier, byEmail);
                DialogResult = true;
            }
            catch (FaultException<FriendFault> friendException)
            {
                ApplicationLogger.LogWarning(
                    "Friend request sending was rejected. Code: {0}",
                    friendException.Detail.ErrorCode);

                ShowSendError();
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(
                    communicationException, "A communication failure prevented the friend request.");

                ShowSendError();
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(
                    timeoutException, "The friend request exceeded the time limit.");

                ShowSendError();
            }
            finally
            {
                buttonAddByEmail.IsEnabled = true;
                buttonAddByCode.IsEnabled = true;
            }
        }

        private void ShowSendError()
        {
            var dialog = new AlertDialog(
                Properties.Resources.TextBlock_FriendRequestSendErrorTitle,
                Properties.Resources.TextBlock_FriendRequestSendErrorMessage,
                AlertType.Error);

            dialog.Owner = this;
            dialog.ShowDialog();
        }

    }
}
