using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using Backgammon.Client.AuthenticationReference;
using Backgammon.Client.Controllers.Profile;
using Backgammon.Client.Controllers.Session;
using Backgammon.Client.ProfileReference;
using Backgammon.Client.Settings.Logging;
using Backgammon.Client.Views.Dialog;
using Backgammon.Client.Views.Models;

namespace Backgammon.Client.Views.Pages
{
    public partial class Avatars : Page
    {
        private static readonly IApplicationLogger ApplicationLogger = new Log4NetApplicationLogger(typeof(Avatars));

        private readonly ProfileController _profileController;
        private string _sessionId = string.Empty;

        public ObservableCollection<AvatarOption> AvailableAvatars { get; } = new ObservableCollection<AvatarOption>();
        public Avatars()
        {
            InitializeComponent();

            _profileController = new ProfileController();
            Loaded += OnAvatarsLoaded;
        }

        public void AddAvatar(int avatarId, byte[] imageData)
        {
            try
            {
                AvatarOption avatar = new AvatarOption(avatarId, imageData);
                AvailableAvatars.Add(avatar);
            }
            catch (NotSupportedException decodingException)
            {
                ApplicationLogger.LogError(
                    decodingException,
                    $"Avatar {avatarId} could not be decoded.");
            }
        }

        private async void OnAvatarsLoaded(object sender, RoutedEventArgs e)
        {
            await LoadAvatarsAsync();
        }

        private async Task LoadAvatarsAsync()
        {
            try
            {
                PlayerSession session = PlayerSessionContext.GetSession();
                _sessionId = session.SessionId;
                Avatar[] avatars =
                    await _profileController.GetAvailableAvatarsAsync(_sessionId);

                AvailableAvatars.Clear();

                foreach (Avatar avatar in avatars)
                {
                    if (avatar != null && avatar.IdAvatar > 0 &&
                        avatar.ImageBytes != null && avatar.ImageBytes.Length > 0)
                    {
                        AddAvatar(avatar.IdAvatar, avatar.ImageBytes);
                    }
                }
            }
            catch (NullReferenceException sessionException)
            {
                ApplicationLogger.LogError(sessionException,
                    "The player session was unavailable while loading avatars.");

                ShowAvatarLoadError();
            }
            catch (FaultException<PlayerProfileFault> profileException)
            {
                ApplicationLogger.LogWarning(
                    "Avatar retrieval was rejected. Code: {0}",
                    profileException.Detail.ErrorCode);

                ShowAvatarLoadError();
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(communicationException,
                    "A communication failure prevented avatars from loading.");

                ShowAvatarLoadError();
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(timeoutException,
                    "The avatar request exceeded the time limit.");

                ShowAvatarLoadError();
            }
        }

        private async void OnAvatarSelected(object sender, SelectionChangedEventArgs e)
        {
            AvatarOption avatar = listBoxAvatars.SelectedItem as AvatarOption;

            if (avatar != null)
            {
                listBoxAvatars.SelectedItem = null;

                Window owner = Window.GetWindow(this);

                if (owner != null)
                {
                    var dialog = new ChangeAvatarDialog(avatar);
                    bool confirmed = dialog.ShowFor(owner);

                    if (confirmed)
                    {
                        await SaveAvatarAsync(avatar.Id);
                    }
                }
            }
        }

        private async Task SaveAvatarAsync(int avatarId)
        {
            listBoxAvatars.IsEnabled = false;

            try
            {
                bool changed = await _profileController.ChangeAvatarAsync(_sessionId, avatarId);

                if (changed && NavigationService != null && NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();
                }
            }
            catch (FaultException<PlayerProfileFault> profileException)
            {
                ApplicationLogger.LogWarning("Avatar change was rejected. Code: {0}",
                    profileException.Detail.ErrorCode);

                ShowAvatarUpdateError();
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(communicationException,
                    "A communication failure prevented the avatar change.");

                ShowAvatarUpdateError();
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(timeoutException,
                    "The avatar change exceeded the time limit.");

                ShowAvatarUpdateError();
            }
            finally
            {
                listBoxAvatars.IsEnabled = true;
            }
        }

        private void ShowAvatarUpdateError()
        {
            MessageBox.Show(
                Properties.Resources.Message_AvatarUpdateError,
                Properties.Resources.Title_AvatarUpdateError,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        private void ShowAvatarLoadError()
        {
            MessageBox.Show(
                Properties.Resources.Message_ProfileLoadError,
                Properties.Resources.Title_ConnectionError,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}
