using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Backgammon.Client.Views.Dialog
{
    /// <summary>
    /// Interaction logic for FriendProfileDialog.xaml
    /// </summary>
    public partial class FriendProfileDialog : Window
    {
        public int TargetAccountId { get; }

        public FriendProfileDialog(
            int accountId, string username, ImageSource avatarImage)
        {
            ValidatePlayer(accountId, username, avatarImage);

            InitializeComponent();

            TargetAccountId = accountId;
            textBlockUsername.Text = username;
            avatarBrush.ImageSource = avatarImage;
        }

        private void ValidatePlayer(
            int accountId, string username, ImageSource avatarImage)
        {
            if (accountId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(accountId));
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException(
                    "A username is required.",
                    nameof(username));
            }

            if (avatarImage == null)
            {
                throw new ArgumentNullException(nameof(avatarImage));
            }
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

        private void OnSendRequestClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
