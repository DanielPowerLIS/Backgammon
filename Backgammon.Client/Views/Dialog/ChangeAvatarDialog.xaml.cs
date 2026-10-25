using System;
using System.Windows;

using Backgammon.Client.Views.Models;

namespace Backgammon.Client.Views.Dialog
{
    public partial class ChangeAvatarDialog : Window
    {

        public ChangeAvatarDialog(AvatarOption selectedAvatar)
        {
            if (selectedAvatar == null)
            {
                throw new ArgumentNullException(nameof(selectedAvatar));
            }

            InitializeComponent();

            SelectedAvatar = selectedAvatar;
            DataContext = selectedAvatar;
        }

        public AvatarOption SelectedAvatar { get; }

        public bool ShowFor(Window owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            Owner = owner;
            return ShowDialog() == true;
        }

        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
