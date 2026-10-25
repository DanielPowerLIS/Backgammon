using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Backgammon.Client.Views.Models;

namespace Backgammon.Client.Views.Dialog
{
    public partial class SelectSocialNetworkDialog : Window
    {

        public SelectSocialNetworkDialog(
            IEnumerable<SocialNetworkOption> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            InitializeComponent();
            itemsControlNetworks.ItemsSource = options.ToList();
        }

        public int SelectedNetworkId { get; private set; }

        public string SelectedNetworkName { get; private set; }
            = string.Empty;

        public bool ShowFor(Window owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            Owner = owner;
            return ShowDialog() == true;
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
    }
}
