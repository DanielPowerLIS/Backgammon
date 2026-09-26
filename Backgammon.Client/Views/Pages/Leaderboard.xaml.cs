using Backgammon.Client.Data;
using Backgammon.Client.Services;
using Backgammon.Client.Utils;
using Backgammon.Client.Views.Dialog;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Backgammon.Client.Views.Pages
{
    public partial class Leaderboard : Page
    {
        private readonly LeaderboardService _leaderboardService;
        public Leaderboard()
        {
            InitializeComponent();

            _leaderboardService = new LeaderboardService();

            Loaded += OnLeaderboardLoaded;
        }

        private void OnLeaderboardLoaded(object sender, RoutedEventArgs e)
        {
            LoadLeaderboard();
        }

        private void LoadLeaderboard()
        {
            try 
            {
                List<Profile> playerProfiles = _leaderboardService.GetLeaderboard();
                LeaderboardListBox.ItemsSource = playerProfiles;
            }catch (SqlException) 
            { 
                ShowLeaderboardError();
            }catch(EntityException)
            {
                ShowLeaderboardError();
            }
        }

        private void ShowLeaderboardError()
        {
            AlertDialog alertDialog = new AlertDialog(
                Properties.Resources.TextBlock_LeaderboardLoadErrorTitle,
                Properties.Resources.TextBlock_LeaderboardLoadErrorMessage,
                Properties.Resources.Button_Accept,
                AlertType.Error);

            alertDialog.Owner = Window.GetWindow(this);
            alertDialog.ShowDialog();
        }
    }
}
