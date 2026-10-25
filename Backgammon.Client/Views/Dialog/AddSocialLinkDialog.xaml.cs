using System;
using System.Windows;

namespace Backgammon.Client.Views.Dialog
{
    public partial class AddSocialLinkDialog : Window
    {

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

    }
}
