using Backgammon.Client.Data;
using System;
using System.Collections.Generic;
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
        private readonly BackgammonEntities context;
        public Leaderboard()
        {
            InitializeComponent();

            context = new BackgammonEntities();

            var accounts = context.Profiles
                .OrderByDescending(a => a.points)
                .ToList();

            LeaderboardListBox.ItemsSource = accounts;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
