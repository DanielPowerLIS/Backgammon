using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

using Backgammon.Client.AuthenticationReference;
using Backgammon.Client.Controllers.Profile;
using Backgammon.Client.Controllers.Session;
using Backgammon.Client.Settings.Logging;

using WcfPlayerProfile = Backgammon.Client.ProfileReference.PlayerProfile;

namespace Backgammon.Client.Views.Pages
{
    public partial class PlayerProfile : Page
    {
        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(PlayerProfile));

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
                PlayerSession currentSession = PlayerSessionContext.GetSession();
                DisplaySessionData(currentSession);

                WcfPlayerProfile profileData =
                    await _profileController.GetPlayerProfileAsync(currentSession.SessionId);

                PopulateProfile(profileData);
            }
            catch (InvalidOperationException sessionException)
            {
                ApplicationLogger.LogError(
                    sessionException,
                    "Invalid session state while attempting to load the player profile.");

                MessageBox.Show(
                    Properties.Resources.Message_ProfileLoadError,
                    Properties.Resources.Title_SessionError,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void DisplaySessionData(PlayerSession currentSession)
        {
            textBlockId.Text = currentSession.PlayerId.ToString();
            textBlockUsername.Text = currentSession.Username;
        }

        private void PopulateProfile(WcfPlayerProfile profileData)
        {
            if (profileData == null)
            {
                MessageBox.Show(
                    Properties.Resources.Message_ProfileLoadError,
                    Properties.Resources.Title_ConnectionError,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            textBlockPoints.Text = profileData.Points.ToString();
            textBlockWins.Text = profileData.Wins.ToString();
            textBlockRegistrationDate.Text = profileData.RegistrationDate.ToShortDateString();

            AssignProfileDescription(profileData.Description);
            AssignProfileAvatar(profileData.Avatar);
        }

        private void AssignProfileAvatar(ProfileReference.Avatar avatar)
        {
            if (avatar == null || avatar.ImageBytes == null || avatar.ImageBytes.Length == 0)
            {
                SetDefaultAvatarAppearance();
                return;
            }

            try
            {
                BitmapImage imageSource = new BitmapImage();

                using (MemoryStream memoryStream = new MemoryStream(avatar.ImageBytes))
                {
                    imageSource.BeginInit();
                    imageSource.CacheOption = BitmapCacheOption.OnLoad;
                    imageSource.StreamSource = memoryStream;
                    imageSource.EndInit();
                }

                imageSource.Freeze();
                ellipseAvatar.Fill = new ImageBrush(imageSource);
            }
            catch (NotSupportedException decodingException)
            {
                ApplicationLogger.LogWarning(
                    "The avatar image bytes could not be decoded. Using default avatar. Details: {0}",
                    decodingException.Message);

                SetDefaultAvatarAppearance();
            }
        }

        private void SetDefaultAvatarAppearance()
        {
            ellipseAvatar.Fill = (Brush)FindResource("CyanAccentBrush");
        }
        private void AssignProfileDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                textBlockDescription.Text = Properties.Resources.TextBlock_NoDescription;
                return;
            }

            textBlockDescription.Text = description;
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void OnAddFacebookClick(object sender, RoutedEventArgs e)
        {
            // TODO: Display dialog to add or edit Facebook social media link.
        }

        private void OnAddInstagramClick(object sender, RoutedEventArgs e)
        {
            // TODO: Display dialog to add or edit Instagram social media link.
        }

        private void OnAddTikTokClick(object sender, RoutedEventArgs e)
        {
            // TODO: Display dialog to add or edit TikTok social media link.
        }
    }
}