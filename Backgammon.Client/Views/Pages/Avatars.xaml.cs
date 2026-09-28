using Backgammon.Client.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Backgammon.Client.Views.Pages
{
    /// <summary>
    /// Interaction logic for Avatars.xaml
    /// </summary>
    public partial class Avatars : Page
    {
        public ObservableCollection<AvatarOption> AvailableAvatars { get; }
            = new ObservableCollection<AvatarOption>();

        public Avatars()
        {
            InitializeComponent();
        }

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
