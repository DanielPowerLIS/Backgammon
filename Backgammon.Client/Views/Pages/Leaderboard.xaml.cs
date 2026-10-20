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
        private readonly IApplicationLogger _applicationLogger;

        public Leaderboard()
            : this(new Log4NetApplicationLogger(typeof(Leaderboard)))
        {
        }

        public Leaderboard(IApplicationLogger applicationLogger)
        {
            InitializeComponent();

            _applicationLogger = applicationLogger
                ?? throw new ArgumentNullException(nameof(applicationLogger));
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
            catch (Exception errorException) when (
                errorException is CommunicationException ||
                errorException is TimeoutException)
            {
                ShowLeaderboardError(errorException);
            }
        }

        private void ShowLeaderboardError(Exception errorException)
        {
            _applicationLogger.LogError(
                errorException,
                "Failed to load the leaderboard from the server.");

            var alertDialog = new AlertDialog(
                Properties.Resources.TextBlock_LeaderboardLoadErrorTitle,
                Properties.Resources.TextBlock_LeaderboardLoadErrorMessage,
                Properties.Resources.Button_Accept,
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
