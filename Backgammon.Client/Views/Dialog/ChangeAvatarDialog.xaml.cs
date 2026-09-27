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


using Backgammon.Client.Models;

namespace Backgammon.Client.Views.Dialog
{
    
    public partial class ChangeAvatarDialog : Window
    {
        public AvatarOption SelectedAvatar { get; }

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
