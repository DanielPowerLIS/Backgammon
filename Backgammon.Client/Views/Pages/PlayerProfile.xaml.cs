using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Backgammon.Client.Controllers.Profile;
using Backgammon.Client.Controllers.Session;
using WcfPlayerProfile = Backgammon.Client.ProfileReference.PlayerProfile;

namespace Backgammon.Client.Views.Pages
{
    public partial class PlayerProfile : Page
    {
        private readonly ProfileController _profileController;

        public PlayerProfile()
        {
            InitializeComponent();
            _profileController = new ProfileController();

            this.Loaded += OnPageLoaded;
        }

        private async void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var currentSession = PlayerSessionContext.GetSession();

                WcfPlayerProfile profileData =
                    await _profileController.GetPlayerProfileAsync(currentSession.SessionId);

                if (profileData != null)
                {
                    PopulateProfileUi(profileData);
                }
                else
                {
                    MessageBox.Show(
                        Properties.Resources.Message_ProfileLoadError,
                        Properties.Resources.Title_ConnectionError,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    Properties.Resources.Title_SessionError,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void PopulateProfileUi(WcfPlayerProfile profileData)
        {
            textBlockPoints.Text = profileData.Points.ToString();
            textBlockWins.Text = profileData.Wins.ToString();

            if (string.IsNullOrWhiteSpace(profileData.Description))
            {
                textBlockDescription.Text = Properties.Resources.TextBlock_NoDescription;
            }
            else
            {
                textBlockDescription.Text = profileData.Description;
            }

            LoadSocialMediaLinks(profileData);

            ellipseAvatar.Fill = (Brush)FindResource("CyanAccentBrush");
        }

        private void LoadSocialMediaLinks(WcfPlayerProfile profile)
        {
            SetSocialMediaLinkText(textBlockFacebook, profile.FacebookLink);
            SetSocialMediaLinkText(textBlockInstagram, profile.InstagramLink);
            SetSocialMediaLinkText(textBlockTikTok, profile.TikTokLink);
        }

        private void SetSocialMediaLinkText(TextBlock textBlock, string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                textBlock.Text = Properties.Resources.TextBlock_NotLinked;
            }
            else
            {
                textBlock.Text = url;
            }
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void OnAddFacebookClick(object sender, RoutedEventArgs e)
        {
            //TODO: Funcionalidad de ventana emergente pendiente
        }

        private void OnAddInstagramClick(object sender, RoutedEventArgs e)
        {
            //TODO: Funcionalidad de ventana emergente pendiente
        }

        private void OnAddTikTokClick(object sender, RoutedEventArgs e)
        {
            //TODO: Funcionalidad de ventana emergente pendiente
        }
    }
}