using System;
using System.Windows;
using System.Windows.Controls;

namespace Backgammon.Client.Views.Dialog
{
    public partial class AddSocialLinkDialog : Window
    {
        private readonly string _socialNetworkName;
        public string Link { get; private set; }
        public AddSocialLinkDialog(string socialNetworkName, string exampleLink)
        {
            if (string.IsNullOrWhiteSpace(socialNetworkName))
            {
                throw new ArgumentException("A social network name is required.", nameof(socialNetworkName));
            }

            if (string.IsNullOrWhiteSpace(exampleLink))
            {
                throw new ArgumentException("An example link is required.", nameof(exampleLink));
            }

            InitializeComponent();
            _socialNetworkName = socialNetworkName;
            Link = string.Empty;
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
            Title = string.Format(Properties.Resources.TextBlock_AddSocialLinkTitleFormat, socialNetworkName);

            textBlockTitle.Text = Title;

            textBlockLinkLabel.Text = string.Format(Properties.Resources.TextBlock_SocialLinkLabelFormat,
                socialNetworkName);

            textBoxLink.Tag = exampleLink;
        }

        private void OnSaveClick(object sender, RoutedEventArgs routedEventArgs)
        {
            string link = textBoxLink.Text.Trim();

            if (!HasValidLink(link))
            {
                MessageBox.Show(Properties.Resources.Message_InvalidSocialLink);
            }
            else
            {
                Link = link;
                DialogResult = true;
            }
        }

        private void OnLinkTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            string link = textBoxLink.Text.Trim();
            buttonSave.IsEnabled = HasValidLink(link);
        }

        private bool HasValidLink(string value)
        {
            Uri linkUri;
            bool isAbsoluteUri = Uri.TryCreate(value, UriKind.Absolute, out linkUri);

            if (!isAbsoluteUri || linkUri.Scheme != Uri.UriSchemeHttps)
            {
                return false;
            }

            string host = linkUri.Host;
            bool validHost = false;

            if (_socialNetworkName == "Facebook")
            {
                validHost = IsSocialHost(host, "facebook.com");
            }
            else if (_socialNetworkName == "Instagram")
            {
                validHost = IsSocialHost(host, "instagram.com");
            }
            else if (_socialNetworkName == "TikTok")
            {
                validHost = IsSocialHost(host, "tiktok.com");
            }

            return validHost;
        }

        private bool IsSocialHost(string host, string domain)
        {
            bool isDomain = string.Equals(host, domain, StringComparison.OrdinalIgnoreCase);
            bool isSubdomain = host.EndsWith("." + domain, StringComparison.OrdinalIgnoreCase);
            bool validHost = isDomain || isSubdomain;

            return validHost;
        }

    }
}
