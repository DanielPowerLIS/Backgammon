using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.ServiceModel;

using Backgammon.Client.AuthenticationReference;
using Backgammon.Client.Controllers.Profile;
using Backgammon.Client.Controllers.Session;
using Backgammon.Client.Settings.Logging;

using ProfileFault = Backgammon.Client.ProfileReference.PlayerProfileFault;
using PlayerProfileData = Backgammon.Client.ProfileReference.PlayerProfile;
using Backgammon.Client.ProfileReference;

namespace Backgammon.Client.Views.Pages
{
    public partial class PlayerProfile : Page
    {
        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(PlayerProfile));

        private readonly ProfileController _profileController;
        private string _description = string.Empty;

        public PlayerProfile()
        {
            InitializeComponent();
            _profileController = new ProfileController();

            Loaded += OnPageLoaded;
        }

        private async void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PlayerSession currentSession = PlayerSessionContext.GetSession();
                DisplaySessionData(currentSession);

                PlayerProfileData profileData = await _profileController.GetPlayerProfileAsync(currentSession.SessionId);

                PopulateProfile(profileData);
            }
            catch (NullReferenceException sessionException)
            {
                ApplicationLogger.LogError(
                    sessionException,
                    "Invalid session state while attempting to load the player profile.");

                ShowProfileLoadError();
            }
            catch (FaultException<ProfileFault> profileFault)
            {
                ApplicationLogger.LogWarning(
                    "The profile request was rejected with code {0}.",
                    profileFault.Detail.ErrorCode);

                ShowProfileLoadError();
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(
                    communicationException,
                    "The player profile request failed.");

                ShowProfileLoadError();
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(
                    timeoutException,
                    "The player profile request exceeded the time limit.");

                ShowProfileLoadError();
            }
        }

        private void OnBackClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void OnEditDescriptionClick(object sender, RoutedEventArgs routedEventArgs)
        {
            textBoxDescription.Text = _description;
            SetDescriptionEditMode(true);
            textBoxDescription.Focus();
        }

        private async void OnSaveDescriptionClick(object sender, RoutedEventArgs routedEventArgs)
        {
            string description = textBoxDescription.Text.Trim();
            buttonSaveDescription.IsEnabled = false;

            try
            {
                PlayerSession currentSession = PlayerSessionContext.GetSession();

                bool updateCompleted = await _profileController.UpdateDescriptionAsync(
                    currentSession.SessionId,
                    description);

                if (updateCompleted)
                {
                    AssignProfileDescription(description);
                    SetDescriptionEditMode(false);
                }
            }
            catch (NullReferenceException sessionException)
            {
                ApplicationLogger.LogError(
                    sessionException,
                    "The profile description could not be updated because the session is unavailable.");

                ShowProfileUpdateError();
            }
            catch (FaultException<ProfileFault> profileFault)
            {
                ApplicationLogger.LogWarning(
                    "The profile update was rejected with code {0}.",
                    profileFault.Detail.ErrorCode);

                ShowProfileUpdateError();
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(
                    communicationException,
                    "The profile description could not be updated.");

                ShowProfileUpdateError();
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(
                    timeoutException,
                    "The profile description update exceeded the time limit.");

                ShowProfileUpdateError();
            }
            finally
            {
                buttonSaveDescription.IsEnabled = true;
            }
        }

        private void OnCancelDescriptionClick(object sender, RoutedEventArgs routedEventArgs)
        {
            textBoxDescription.Text = _description;
            SetDescriptionEditMode(false);
        }

       private void OnAddFacebookClick(object sender, RoutedEventArgs routedEventArgs)
        {
            // TODO: Display dialog to add or edit Facebook social media link.
        }

        private void OnAddInstagramClick(object sender, RoutedEventArgs routedEventArgs)
        {
            // TODO: Display dialog to add or edit Instagram social media link.
        }

        private void OnAddTikTokClick(object sender, RoutedEventArgs routedEventArgs)
        {
            // TODO: Display dialog to add or edit TikTok social media link.
        }
        private void SetDescriptionEditMode(bool isEditing)
        {
            if (isEditing)
            {
                textBlockDescription.Visibility = Visibility.Collapsed;
                textBoxDescription.Visibility = Visibility.Visible;
                buttonEditDescription.Visibility = Visibility.Collapsed;
                buttonSaveDescription.Visibility = Visibility.Visible;
                buttonCancelDescription.Visibility = Visibility.Visible;
            }
            else
            {
                textBlockDescription.Visibility = Visibility.Visible;
                textBoxDescription.Visibility = Visibility.Collapsed;
                buttonEditDescription.Visibility = Visibility.Visible;
                buttonSaveDescription.Visibility = Visibility.Collapsed;
                buttonCancelDescription.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowProfileUpdateError()
        {
            MessageBox.Show(
                Properties.Resources.Message_ProfileUpdateError,
                Properties.Resources.Title_ConnectionError,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }


        private void ShowProfileLoadError()
        {
            MessageBox.Show(
                Properties.Resources.Message_ProfileLoadError,
                Properties.Resources.Title_ConnectionError,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        private void DisplaySessionData(PlayerSession currentSession)
        {
            textBlockId.Text = currentSession.PlayerId.ToString();
            textBlockUsername.Text = currentSession.Username;
        }

        private void PopulateProfile(PlayerProfileData profileData)
        {
            if (profileData == null)
            {
                MessageBox.Show(
                    Properties.Resources.Message_ProfileLoadError,
                    Properties.Resources.Title_ConnectionError,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            else
            {
                buttonEditDescription.IsEnabled = true;
                textBlockPoints.Text = profileData.Points.ToString();
                textBlockWins.Text = profileData.Wins.ToString();
                textBlockRegistrationDate.Text = profileData.RegistrationDate.ToShortDateString();

                AssignProfileDescription(profileData.Description);
                AssignProfileAvatar(profileData.Avatar);
            }
        }

        private void AssignProfileAvatar(Avatar avatar)
        {
            if (avatar == null || avatar.ImageBytes == null || avatar.ImageBytes.Length == 0)
            {
                SetDefaultAvatarAppearance();
            }
            else
            {
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
        }

        private void SetDefaultAvatarAppearance()
        {
            ellipseAvatar.Fill = (Brush)FindResource("CyanAccentBrush");
        }
        private void AssignProfileDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                _description = string.Empty;
                textBlockDescription.Text = Properties.Resources.TextBlock_NoDescription;
            }
            else
            {
                _description = description;
                textBlockDescription.Text = description;
            }
        }
    }
}
