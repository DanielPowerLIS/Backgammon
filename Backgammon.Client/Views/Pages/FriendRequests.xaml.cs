using Backgammon.Client.Controllers.Friends;
using Backgammon.Client.FriendReference;
using Backgammon.Client.Settings.Logging;
using Backgammon.Client.Views.Dialog;
using Backgammon.Client.Views.Models;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Backgammon.Client.Views.Pages
{
    public partial class FriendRequests : Page
    {
        private readonly FriendController _friendController;
        private static readonly IApplicationLogger ApplicationLogger = 
            new Log4NetApplicationLogger(typeof(FriendRequests));
        public FriendRequests()
        {
            InitializeComponent();

            _friendController = new FriendController();
            Loaded += OnFriendRequestsLoaded;
        }

        private async void OnFriendRequestsLoaded(object sender, RoutedEventArgs e)
        {
            await LoadFriendRequestsAsync();
        }

        private async Task LoadFriendRequestsAsync()
        {
            try
            {
                FriendRequestPending[] friendRequests = await _friendController.GetFriendRequestsAsync();
                listViewFriendRequests.ItemsSource = friendRequests;
            }
            catch (FaultException<FriendFault> friendException)
            {
                ApplicationLogger.LogError(friendException, 
                    "The server could not load the friend requests list. Code: {0}",
                    friendException.Detail.ErrorCode);

                ShowFriendRequestsError();
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(communicationException, 
                    "The server could not load the friend requests list. Communication error: {0}",
                    communicationException.Message);

                ShowFriendRequestsError();
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(timeoutException, 
                    "The server could not load the friend requests list. Timeout error: {0}",
                    timeoutException.Message);

                ShowFriendRequestsError();
            }

        }

        private void ShowFriendRequestsError()
        {
            var alertDialog = new AlertDialog(
                Properties.Resources.TextBlock_FriendRequestErrorTitle,
                Properties.Resources.TextBlock_FriendRequestErrorMessage,
                AlertType.Error);

            alertDialog.Owner = Window.GetWindow(this);
            alertDialog.ShowDialog();
        }

        private void OnBackClick(object sender, RoutedEventArgs eventArgs)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

    }
}
