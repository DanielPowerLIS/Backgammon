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
using System.Windows.Navigation;

namespace Backgammon.Client.Views.Pages
{
    public partial class Friends : Page
    {
        private readonly FriendController _friendController;
        private static readonly IApplicationLogger ApplicationLogger = new Log4NetApplicationLogger(typeof(Friends));
        public Friends()
        {
            InitializeComponent();
            _friendController = new FriendController();
            Loaded += OnFriendsLoaded;
        }

        private void OnBackClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (NavigationService.CanGoBack) 
            {
                NavigationService.GoBack();
            }
        }

        private void OnFriendRequest(object sender, RoutedEventArgs routedEventArgs)
        {
            FriendRequests friendRequestPage = new FriendRequests();

            NavigationService.Navigate(friendRequestPage);
        }

        private void OnAddFriend(object sender, RoutedEventArgs routedEventArgs)
        {
            Window owner = Window.GetWindow(this);
            var dialog = new AddFriendDialog();

            bool requestSent = dialog.ShowFor(owner);

            if (requestSent)
            {
                var confirmation = new AlertDialog(
                    Properties.Resources.TextBlock_FriendRequestSentTitle,
                    Properties.Resources.TextBlock_FriendRequestSentMessage,
                    AlertType.Normal);

                confirmation.Owner = owner;
                confirmation.ShowDialog();
            }
        }

        private async void OnFriendsLoaded(object sender, RoutedEventArgs eventArgs)
        {
            await LoadFriendsAsync();
        }

        private async Task LoadFriendsAsync()
        {
            try
            {
                Friend[] friends = await _friendController.GetFriendsAsync();
                listViewFriends.ItemsSource = friends;
            }
            catch (FaultException<FriendFault> friendException)
            {
                ApplicationLogger.LogError(friendException, "The server could not load the friends list. Code: {0}",
                    friendException.Detail.ErrorCode);

                ShowFriendsError();
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(communicationException,
                    "A communication failure prevented the friends list from loading.");

                ShowFriendsError();
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(timeoutException, "The friends list request exceeded the time limit.");

                ShowFriendsError();
            }
        }

        private async void OnRemoveFriendClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Friend friend = (Friend)button.DataContext;

            string message = string.Format(
                Properties.Resources.TextBlock_RemoveFriendConfirmationMessage,
                friend.Username);

            Window owner = Window.GetWindow(this);
            var confirmation = new ConfirmationDialog(
                Properties.Resources.Title_FriendRemoval,
                message);
            bool confirmed = confirmation.ShowOver(owner);

            if (confirmed)
            {
                await RemoveFriendAsync(friend.AccountId);
            }
        }

        private async Task RemoveFriendAsync(int friendAccountId)
        {
            listViewFriends.IsEnabled = false;

            try
            {
                await _friendController.RemoveFriendAsync(friendAccountId);
                await LoadFriendsAsync();
            }
            catch (FaultException<FriendFault> friendException)
            {
                ApplicationLogger.LogWarning(
                    "Friend removal was rejected. Code: {0}",
                    friendException.Detail.ErrorCode);

                ShowFriendRemovalError();
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(
                    communicationException, "A communication failure prevented friendship removal.");

                ShowFriendRemovalError();
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(
                    timeoutException, "Friendship removal exceeded the time limit.");

                ShowFriendRemovalError();
            }
            finally
            {
                listViewFriends.IsEnabled = true;
            }
        }

        private void ShowFriendRemovalError()
        {
            var dialog = new AlertDialog(
                Properties.Resources.TextBlock_FriendRemovalErrorTitle,
                Properties.Resources.TextBlock_FriendRemovalErrorMessage,
                AlertType.Error);

            dialog.Owner = Window.GetWindow(this);
            dialog.ShowDialog();
        }

        private void ShowFriendsError()
        {
            var alertDialog = new AlertDialog(
                Properties.Resources.TextBlock_FriendsRetrievalErrorTitle,
                Properties.Resources.TextBlock_FriendsRetrievalErrorMessage,
                AlertType.Error);

            alertDialog.Owner = Window.GetWindow(this);
            alertDialog.ShowDialog();
        }
    }
}
