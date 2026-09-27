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
using System.Windows.Controls;

namespace Backgammon.Client.Views.Dialog
{
    /// <summary>
    /// Interaction logic for AddSocialLinkDialog.xaml
    /// </summary>
    public partial class AddSocialLinkDialog : Window
    {
        public string Link { get; private set; } = string.Empty;

        public AddSocialLinkDialog(
            string socialNetworkName, string exampleLink)
        {
            if (string.IsNullOrWhiteSpace(socialNetworkName))
            {
                throw new ArgumentException(
                    "A social network name is required.",
                    nameof(socialNetworkName));
            }

            if (string.IsNullOrWhiteSpace(exampleLink))
            {
                throw new ArgumentException(
                    "An example link is required.",
                    nameof(exampleLink));
            }

            InitializeComponent();
            SetDialogText(socialNetworkName, exampleLink);
            textBoxLink.TextChanged += OnLinkTextChanged;
        }

        private void SetDialogText(
            string socialNetworkName, string exampleLink)
        {
            Title = string.Format(
                Properties.Resources.TextBlock_AddSocialLinkTitleFormat,
                socialNetworkName);

            textBlockTitle.Text = Title;

            textBlockLinkLabel.Text = string.Format(
                Properties.Resources.TextBlock_SocialLinkLabelFormat,
                socialNetworkName);

            textBoxLink.Tag = exampleLink;
        }

        private void OnLinkTextChanged(
            object sender, TextChangedEventArgs e)
        {
            buttonSave.IsEnabled =
                !string.IsNullOrWhiteSpace(textBoxLink.Text);
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxLink.Text))
            {
                return;
            }

            Link = textBoxLink.Text.Trim();
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
