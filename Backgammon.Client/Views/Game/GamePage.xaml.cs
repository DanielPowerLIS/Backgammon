using System.Windows;
using System.Windows.Controls;

using Backgammon.Client.Controllers.Game;
using Backgammon.Client.MatchReference;
using Backgammon.Client.Views.Dialog;

namespace Backgammon.Client.Views.Game
{
    public partial class GamePage : Page
    {
        public MatchSession CurrentMatch { get; }

        public GamePage(MatchSession matchSession)
        {
            InitializeComponent();

            CurrentMatch = matchSession;

            BoardState boardState = BoardState.CreateInitial();
            boardControlGame.RenderBoard(boardState);
        }

        private void OnRollDiceButtonClick(object sender, RoutedEventArgs e)
        {
            var diceDialog = new DiceRollDialog(PlayerColor.White);
            diceDialog.Owner = Window.GetWindow(this);
            diceDialog.ShowDialog();

            DiceResult result = diceDialog.Result;
        }
    }
}
