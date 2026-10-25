using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using Backgammon.Client.Controllers.Leaderboard;
using Backgammon.Client.LeaderboardReference;
using Backgammon.Client.Settings.Logging;
using Backgammon.Client.Views.Models;
using Backgammon.Client.Views.Dialog;

namespace Backgammon.Client.Views.Pages
{
    public partial class Leaderboard : Page
    {
        private readonly LeaderboardController _leaderboardController;
        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(Leaderboard));

        public Leaderboard()
        {
            InitializeComponent();

            _leaderboardController = new LeaderboardController();

            Loaded += OnLeaderboardLoaded;
        }

        private async void OnLeaderboardLoaded(
            object eventSender,
            RoutedEventArgs eventArgs)
        {
            await LoadLeaderboardAsync();
        }

        private async Task LoadLeaderboardAsync()
        {
            try
            {
                PlayerStanding[] leaderboardEntries =
                    await _leaderboardController.GetLeaderboardAsync();

                listBoxLeaderboard.ItemsSource = leaderboardEntries;
            }
            catch (FaultException<LeaderboardFault> leaderboardException)
            {
                ShowLeaderboardError(leaderboardException);
            }
            catch (Exception connectionException) when (
                connectionException is CommunicationException ||
                connectionException is TimeoutException)
            {
                ShowLeaderboardError(connectionException);
            }
        }

        private void ShowLeaderboardError(Exception errorException)
        {
            ApplicationLogger.LogError(
                errorException,
                "Failed to load the leaderboard from the server.");

            var alertDialog = new AlertDialog(
                Properties.Resources.TextBlock_LeaderboardLoadErrorTitle,
                Properties.Resources.TextBlock_LeaderboardLoadErrorMessage,
                AlertType.Error);

            alertDialog.Owner = Window.GetWindow(this);
            alertDialog.ShowDialog();
        }

        private void OnBackClick(
            object eventSender,
            RoutedEventArgs eventArgs)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}
