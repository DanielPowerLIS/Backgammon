using System;
using System.Windows;
using System.Windows.Controls;

using Backgammon.Client.Controllers.Data;

namespace Backgammon.Client.Views.Dialog
{
    public partial class AddFriendDialog : Window
    {
        public string SearchValue { get; private set; } = string.Empty;

        public FriendSearchMode SearchMode { get; private set; }
            = FriendSearchMode.None;

        public AddFriendDialog()
        {
            InitializeComponent();
            textBoxSearchValue.TextChanged += OnSearchValueChanged;
        }

        private void OnAddByEmailClick(object sender, RoutedEventArgs e)
        {
            CompleteSearch(FriendSearchMode.Email);
        }

        private void OnAddByCodeClick(object sender, RoutedEventArgs e)
        {
            CompleteSearch(FriendSearchMode.AccountCode);
        }

        private void CompleteSearch(FriendSearchMode searchMode)
        {
            string enteredValue = textBoxSearchValue.Text.Trim();

            if (string.IsNullOrWhiteSpace(enteredValue))
            {
                textBlockRequiredMessage.Visibility = Visibility.Visible;
                textBoxSearchValue.Focus();
                return;
            }

            SearchValue = enteredValue;
            SearchMode = searchMode;
            DialogResult = true;
        }

        private void OnSearchValueChanged(
            object sender, TextChangedEventArgs e)
        {
            textBlockRequiredMessage.Visibility = Visibility.Hidden;
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
