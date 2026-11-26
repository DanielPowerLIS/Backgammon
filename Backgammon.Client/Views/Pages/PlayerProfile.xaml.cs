using Backgammon.Client.AuthenticationReference;
using Backgammon.Client.Controllers.Profile;
using Backgammon.Client.Controllers.Session;
using Backgammon.Client.ProfileReference;
using Backgammon.Client.Settings.Logging;
using Backgammon.Client.Views.Dialog;
using Backgammon.Client.Views.Models;

using System;
using System.IO;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using PlayerProfileData = Backgammon.Client.ProfileReference.PlayerProfile;
using ProfileFault = Backgammon.Client.ProfileReference.PlayerProfileFault;

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

                PlayerProfileData profileData = await _profileController.GetPlayerProfileAsync(
                    currentSession.SessionId);

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

        private async void OnAddFacebookClick(object sender, RoutedEventArgs routedEventArgs)
        {
            await ManageSocialLinkAsync("Facebook", "https://www.facebook.com/", buttonAddFacebook);
        }

        private async void OnAddInstagramClick(object sender, RoutedEventArgs routedEventArgs)
        {
            await ManageSocialLinkAsync("Instagram", "https://www.instagram.com/", buttonAddInstagram);
        }

        private async void OnAddTikTokClick(object sender, RoutedEventArgs routedEventArgs)
        {
            await ManageSocialLinkAsync("TikTok", "https://www.tiktok.com/", buttonAddTikTok);
        }

        private void OnAvatarClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (NavigationService != null)
            {
                NavigationService.Navigate(new Avatars());
            }
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
            var dialog = new AlertDialog(
                Properties.Resources.Title_ConnectionError,
                Properties.Resources.Message_ProfileUpdateError,
                AlertType.Error);

            dialog.Owner = Window.GetWindow(this);
            dialog.ShowDialog();
        }


        private void ShowProfileLoadError()
        {
            var dialog = new AlertDialog(
                Properties.Resources.Title_ConnectionError,
                Properties.Resources.Message_ProfileLoadError,
                AlertType.Error);

            dialog.Owner = Window.GetWindow(this);
            dialog.ShowDialog();
        }

        private void DisplaySessionData(PlayerSession currentSession)
        {
            textBlockUsername.Text = currentSession.Username;
        }

        private void PopulateProfile(PlayerProfileData profileData)
        {
            if (profileData == null)
            {
                ShowProfileLoadError();
            }
            else
            {
                buttonEditDescription.IsEnabled = true;
                textBlockId.Text = profileData.AccountCode;
                textBlockPoints.Text = profileData.Points.ToString();
                textBlockWins.Text = profileData.Wins.ToString();
                textBlockRegistrationDate.Text = profileData.RegistrationDate.ToShortDateString();

                AssignProfileDescription(profileData.Description);
                AssignProfileAvatar(profileData.Avatar);

                var facebookLink = new SocialMediaLink
                {
                    IdSocialMedia = profileData.FacebookLinkId,
                    Url = profileData.FacebookLink
                };
                AssignSocialLink(textBlockFacebook, buttonAddFacebook, facebookLink);

                var instagramLink = new SocialMediaLink
                {
                    IdSocialMedia = profileData.InstagramLinkId,
                    Url = profileData.InstagramLink
                };
                AssignSocialLink(textBlockInstagram, buttonAddInstagram, instagramLink);

                var tikTokLink = new SocialMediaLink
                {
                    IdSocialMedia = profileData.TikTokLinkId,
                    Url = profileData.TikTokLink
                };
                AssignSocialLink(textBlockTikTok, buttonAddTikTok, tikTokLink);
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
                MemoryStream memoryStream = new MemoryStream(avatar.ImageBytes);

                try
                {
                    BitmapImage imageSource = new BitmapImage();

                    imageSource.BeginInit();
                    imageSource.CacheOption = BitmapCacheOption.OnLoad;
                    imageSource.StreamSource = memoryStream;
                    imageSource.EndInit();

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
                finally
                {
                    memoryStream.Dispose();
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

        private async Task ManageSocialLinkAsync(string networkName, string exampleLink, Button linkButton)
        {
            try
            {
                TextBlock linkText = GetSocialLinkText(networkName);

                if (string.IsNullOrWhiteSpace(linkText.Text))
                {
                    await AddSocialLinkWithFeedbackAsync(
                        networkName,
                        exampleLink,
                        linkButton);
                }
                else
                {
                    await RemoveSocialLinkAsync(linkText, linkButton);
                }
            }
            catch (ArgumentException argumentException)
            {
                ApplicationLogger.LogError(argumentException,
                    "The social media action could not be completed because the network is unsupported.");

                ShowSocialLinkError();
            }
        }

        private TextBlock GetSocialLinkText(string networkName)
        {
            if (networkName == "Facebook")
            {
                TextBlock linkText = textBlockFacebook;
                return linkText;
            }

            if (networkName == "Instagram")
            {
                TextBlock linkText = textBlockInstagram;
                return linkText;
            }

            if (networkName == "TikTok")
            {
                TextBlock linkText = textBlockTikTok;
                return linkText;
            }

            throw new ArgumentException(nameof(networkName));
        }

        private async Task AddSocialLinkWithFeedbackAsync(string networkName, string exampleLink, Button linkButton)
        {
            linkButton.IsEnabled = false;

            try
            {
                await SaveSocialLinkAsync(networkName, exampleLink, linkButton);
            }
            catch (NullReferenceException sessionException)
            {
                ApplicationLogger.LogError(
                    sessionException,
                    "The social media link could not be added because the session is unavailable.");

                ShowSocialLinkError();
            }
            catch (FaultException<ProfileFault> profileFault)
            {
                ApplicationLogger.LogWarning(
                    "The social media link request was rejected with code {0}.",
                    profileFault.Detail.ErrorCode);

                ShowSocialLinkError();
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(
                    communicationException,
                    "The social media link could not be added.");

                ShowSocialLinkError();
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(
                    timeoutException,
                    "The social media link request exceeded the time limit.");

                ShowSocialLinkError();
            }
            finally
            {
                linkButton.IsEnabled = true;
            }
        }

        private async Task SaveSocialLinkAsync(string networkName, string exampleLink, Button linkButton)
        {
            TextBlock linkText = GetSocialLinkText(networkName);

            if (!string.IsNullOrWhiteSpace(linkText.Text))
            {
                return;
            }

            PlayerSession currentSession = PlayerSessionContext.GetSession();
            SocialMediaType[] socialTypes = await _profileController.GetSocialMediaTypesAsync(currentSession.SessionId);
            int typeId = GetSocialMediaTypeId(socialTypes, networkName);

            if (typeId <= 0)
            {
                ShowSocialLinkError();
                return;
            }

            var dialog = new AddSocialLinkDialog(networkName, exampleLink);
            Window owner = Window.GetWindow(this);
            bool confirmed = dialog.ShowFor(owner);

            if (!confirmed)
            {
                return;
            }

            SocialMediaLink addedLink = await _profileController.AddSocialMediaLinkAsync(currentSession.SessionId,
                typeId,
                dialog.Link);

            if (addedLink == null || string.IsNullOrWhiteSpace(addedLink.Url))
            {
                ShowSocialLinkError();
                return;
            }

            AssignSocialLink(linkText, linkButton, addedLink);
        }

        private async Task RemoveSocialLinkAsync(TextBlock linkText, Button linkButton)
        {
            bool confirmed = ConfirmSocialLinkRemoval();

            if (!confirmed)
            {
                return;
            }

            int linkId = GetSocialLinkId(linkButton);

            if (linkId <= 0)
            {
                ApplicationLogger.LogWarning(
                    "The social media link could not be removed because its ID is unavailable.");

                ShowSocialLinkError();
                return;
            }

            linkButton.IsEnabled = false;
            await RemoveSocialLinkWithFeedbackAsync(linkText, linkButton, linkId);
        }

        private async Task RemoveSocialLinkWithFeedbackAsync(TextBlock linkText, Button linkButton, int linkId)
        {
            try
            {
                await DeleteSocialLinkAsync(linkText, linkButton, linkId);
            }
            catch (NullReferenceException sessionException)
            {
                ApplicationLogger.LogError(
                    sessionException,
                    "The social media link could not be removed because the session is unavailable.");

                ShowSocialLinkError();
            }
            catch (FaultException<ProfileFault> profileFault)
            {
                ApplicationLogger.LogWarning(
                    "The social media link removal was rejected with code {0}.",
                    profileFault.Detail.ErrorCode);

                ShowSocialLinkError();
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(
                    communicationException,
                    "The social media link could not be removed.");

                ShowSocialLinkError();
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(
                    timeoutException,
                    "The social media link removal exceeded the time limit.");

                ShowSocialLinkError();
            }
            finally
            {
                linkButton.IsEnabled = true;
            }
        }

        private async Task DeleteSocialLinkAsync(TextBlock linkText, Button linkButton, int linkId)
        {
            PlayerSession currentSession = PlayerSessionContext.GetSession();
            bool removalCompleted = await _profileController.RemoveSocialMediaLinkAsync(currentSession.SessionId,
                linkId);

            if (removalCompleted)
            {
                var emptyLink = new SocialMediaLink();
                AssignSocialLink(linkText, linkButton, emptyLink);
            }
        }

        private bool ConfirmSocialLinkRemoval()
        {
            var dialog = new ConfirmationDialog(
                Properties.Resources.Title_SocialLinkRemoval,
                Properties.Resources.Message_ConfirmSocialLinkRemoval);

            Window owner = Window.GetWindow(this);
            bool confirmed = dialog.ShowOver(owner);

            return confirmed;
        }

        private int GetSocialLinkId(Button linkButton)
        {
            int linkId = 0;

            if (linkButton.Tag is int storedLinkId)
            {
                linkId = storedLinkId;
            }

            return linkId;
        }

        private void ShowSocialLinkError()
        {
            var dialog = new AlertDialog(
                Properties.Resources.Title_ConnectionError,
                Properties.Resources.Message_SocialLinkUpdateError,
                AlertType.Error);

            dialog.Owner = Window.GetWindow(this);
            dialog.ShowDialog();
        }

        private int GetSocialMediaTypeId(SocialMediaType[] socialTypes, string networkName)
        {
            int typeId = 0;

            if (socialTypes != null)
            {
                foreach (SocialMediaType socialType in socialTypes)
                {
                    if (socialType != null && string.Equals(socialType.Type, networkName,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        typeId = socialType.IdType;
                        break;
                    }
                }
            }

            return typeId;
        }

        private void AssignSocialLink(TextBlock linkText, Button linkButton, SocialMediaLink socialLink)
        {
            if (string.IsNullOrWhiteSpace(socialLink.Url))
            {
                linkText.Text = string.Empty;
                linkButton.Tag = 0;
                linkButton.Content = Properties.Resources.Button_AddLink;
                linkButton.IsEnabled = true;
            }
            else
            {
                linkText.Text = socialLink.Url;
                linkButton.Tag = socialLink.IdSocialMedia;
                linkButton.Content = Properties.Resources.Button_Remove;
                linkButton.IsEnabled = socialLink.IdSocialMedia > 0;
            }
        }

    }
}
