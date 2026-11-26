using System;
using System.Windows;

namespace Backgammon.Client.Views.Dialog
{
    public partial class ConfirmationDialog : Window
    {
        public ConfirmationDialog(string title, string message)
        {
            InitializeComponent();

            textBlockTitle.Text = title;
            textBlockMessage.Text = message;
        }

        public bool ShowOver(Window owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            Owner = owner;

            bool confirmed = ShowDialog() == true;

            return confirmed;
        }

        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
