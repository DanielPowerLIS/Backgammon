using System;
using System.Windows;

namespace Backgammon.Client.Views.Dialog
{
    public partial class AddFriendDialog : Window
    {

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
    }
}
