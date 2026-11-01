using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

using Backgammon.Client.Controllers.Preinitiation;
using Backgammon.Client.Views.Dialog;
using Backgammon.Client.Controllers.Data;
using Backgammon.Client.PreinitiationReference;
using Backgammon.Client.Settings.Logging;
using Backgammon.Client.Views.Models;

namespace Backgammon.Client.Views.Pages
{
    public partial class Preinitiation : Page
    {
        private readonly PreinitiationController _preinitiationController;

        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(Preinitiation));

        private static readonly string JoinLobbyErrorCode = "LobbyJoinFailed";
        private static readonly string SessionExpiredErrorCode = "SessionExpired";
        private static readonly string InvalidRoomCodeErrorCode = "InvalidRoomCode";

        public Preinitiation()
        {
            InitializeComponent();
            _preinitiationController = new PreinitiationController();
        }

        private void OnCreateLobbyClick(object sender, RoutedEventArgs e)
        {
            if (_preinitiationController.CreateLobby())
            {
                NavigationService.Navigate(new Lobby());
            }
        }

        private async void OnJoinClick(object sender, RoutedEventArgs e)
        {
            var joinLobbyDialog = new JoinLobbyDialog
            {
                Owner = Window.GetWindow(this)
            };

            joinLobbyDialog.ShowDialog();
            bool confirmed = joinLobbyDialog.IsConfirmed;

            if (!confirmed)
            {
                return;
            }

            IsEnabled = false;
            try
            {
                await TryJoinLobbyAsync(joinLobbyDialog.RoomCode);
            }
            finally
            {
                IsEnabled = true;
            }
        }

        private async Task TryJoinLobbyAsync(string roomCode)
        {
            try
            {
                await CompleteJoinLobbyAsync(roomCode);
            }
            catch (FaultException<PreinitiationFault> lobbyException)
            {
                string errorCode = lobbyException.Detail.ErrorCode;

                if (errorCode == JoinLobbyErrorCode ||
                    errorCode == SessionExpiredErrorCode ||
                    errorCode == InvalidRoomCodeErrorCode)
                {
                    ApplicationLogger.LogWarning(
                        "The lobby join request was rejected. Code: {0}",
                        errorCode);
                }
                else
                {
                    ApplicationLogger.LogError(lobbyException,
                        "The server could not complete the lobby join request. Code: {0}",
                        errorCode);
                }

                ShowJoinMessage(Properties.Resources.TextBlock_JoinLobbyErrorMessage);
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(
                    communicationException,
                    "A communication failure prevented the player from joining the lobby.");

                ShowJoinMessage(Properties.Resources.TextBlock_JoinLobbyErrorMessage);
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(
                    timeoutException,
                    "The lobby join request exceeded the time limit.");

                ShowJoinMessage(Properties.Resources.TextBlock_JoinLobbyErrorMessage);
            }
        }

        private async Task CompleteJoinLobbyAsync(string roomCode)
        {
            InputValidationResult validationResult = await _preinitiationController.JoinLobbyAsync(roomCode);

            if (!validationResult.IsValid)
            {
                ShowJoinMessage(Properties.Resources.TextBlock_InvalidRoomCodeMessage);
                return;
            }

            IsEnabled = true;
            NavigationService.Navigate(new Lobby());
        }

        private void ShowJoinMessage(string message)
        {
            var alertDialog = new AlertDialog(Properties.Resources.TextBlock_JoinLobbyTitle, message, AlertType.Error)
            {
                Owner = Window.GetWindow(this)
            };

            alertDialog.ShowDialog();
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack) 
            { 
                NavigationService.GoBack();
            }
        }
    }
}
