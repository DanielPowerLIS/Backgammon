using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using Backgammon.Client.Views.Models;

namespace Backgammon.Client.Views.Pages
{
    public partial class Avatars : Page
    {

        public Avatars()
        {
            InitializeComponent();
        }

        public ObservableCollection<AvatarOption> AvailableAvatars { get; }
            = new ObservableCollection<AvatarOption>();

        public void AddAvatar(int avatarId, byte[] imageData)
        {
            AvatarOption avatar = new AvatarOption(avatarId, imageData);
            AvailableAvatars.Add(avatar);
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}
