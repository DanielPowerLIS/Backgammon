using Backgammon.Client.Utils;
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
