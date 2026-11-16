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

        public Preinitiation()
        {
            InitializeComponent();
            _preinitiationController = new PreinitiationController();
        }

        private void OnCreateLobbyClick(object sender, RoutedEventArgs e)
        {
            try
            {
                bool lobbyCreated = _preinitiationController.CreateLobby();

                if (lobbyCreated)
                {
                    NavigationService.Navigate(new Lobby());
                }
            }
            catch (FaultException<PreinitiationFault> lobbyFault)
            {
                ApplicationLogger.LogWarning(
                    "Lobby creation was rejected with code {0}.",
                    lobbyFault.Detail.ErrorCode);

                ShowLobbyCreationError();
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(
                    communicationException,
                    "The lobby creation request failed.");

                ShowLobbyCreationError();
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(
                    timeoutException,
                    "The lobby creation request exceeded the time limit.");

                ShowLobbyCreationError();
            }
            catch (NullReferenceException sessionException)
            {
                ApplicationLogger.LogError(
                    sessionException,
                    "A player session was unavailable while creating the lobby.");

                ShowLobbyCreationError();
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
                await JoinLobbyAsync(joinLobbyDialog.RoomCode);
            }
            finally
            {
                IsEnabled = true;
            }
        }

        private async Task JoinLobbyAsync(string roomCode)
        {
            try
            {
                await CompleteJoinLobbyAsync(roomCode);
            }
            catch (FaultException<PreinitiationFault> lobbyException)
            {
                ApplicationLogger.LogWarning("Lobby join request was rejected with code {0}.",
                    lobbyException.Detail.ErrorCode);

                ShowJoinMessage();
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(
                    communicationException,
                    "A communication failure prevented the player from joining the lobby.");

                ShowJoinMessage();
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(
                    timeoutException,
                    "The lobby join request exceeded the time limit.");

                ShowJoinMessage();
            }
            catch (NullReferenceException sessionException)
            {
                ApplicationLogger.LogError(
                    sessionException,
                    "A player session was unavailable while joining the lobby.");

                ShowJoinMessage();
            }
        }

        private async Task CompleteJoinLobbyAsync(string roomCode)
        {
            InputValidationResult validationResult = await _preinitiationController.JoinLobbyAsync(roomCode);

            if (!validationResult.IsValid)
            {
                ShowJoinMessage();
                return;
            }

            IsEnabled = true;
            NavigationService.Navigate(new Lobby(_preinitiationController));
        }

        private void ShowLobbyCreationError()
        {
            var alertDialog = new AlertDialog(Properties.Resources.TextBlock_LobbyStartErrorTitle,
                Properties.Resources.TextBlock_LobbyStartErrorMessage,
                AlertType.Error)
            {
                Owner = Window.GetWindow(this)
            };

            alertDialog.ShowDialog();
        }

        private void ShowJoinMessage()
        {
            var alertDialog = new AlertDialog(Properties.Resources.TextBlock_JoinLobbyTitle, 
                Properties.Resources.TextBlock_JoinLobbyErrorMessage,
                AlertType.Error)
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
