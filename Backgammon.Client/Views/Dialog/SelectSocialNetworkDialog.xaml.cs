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
    /// <summary>
    /// Interaction logic for SelectSocialNetworkDialog.xaml
    /// </summary>
    public partial class SelectSocialNetworkDialog : Window
    {
        public int SelectedNetworkId { get; private set; }

        public string SelectedNetworkName { get; private set; }
            = string.Empty;

        public SelectSocialNetworkDialog(
            IEnumerable<SocialNetworkOption> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            InitializeComponent();
            networkItemsControl.ItemsSource = options.ToList();
        }

        private void OnNetworkClick(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button) ||
                !(button.DataContext is SocialNetworkOption option) ||
                !option.IsAvailable)
            {
                return;
            }

            SelectedNetworkId = option.Id;
            SelectedNetworkName = option.Name;
            DialogResult = true;
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
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
