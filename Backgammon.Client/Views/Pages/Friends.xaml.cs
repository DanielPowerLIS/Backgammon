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

        }

        private void OnAddFriend(object sender, RoutedEventArgs routedEventArgs)
        {

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
