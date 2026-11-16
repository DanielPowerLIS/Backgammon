using System;
using System.Windows;

namespace Backgammon.Client.Views.Dialog
{
    public partial class AccountDialog : Window
    {
        public AccountDialog(string heading, string message)
        {
            InitializeComponent();

            textBlockHeading.Text = heading;
            textBlockMessage.Text = message;
        }

        public void ShowOver(Window owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            Owner = owner;

            ShowDialog();
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
