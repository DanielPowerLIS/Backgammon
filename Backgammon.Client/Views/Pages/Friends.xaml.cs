using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;


using Backgammon.Client.Views.Dialog;

namespace Backgammon.Client.Views.Pages
{
    public partial class Friends : Page
    {
        public Friends()
        {
            InitializeComponent();
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
    }
}
