using System.Windows;
using System.Windows.Controls;

using Backgammon.Client.Controllers.Game;
using Backgammon.Client.Views.Dialog;

namespace Backgammon.Client.Views.Game
{
    public partial class GamePage : Page
    {
        public GamePage()
        {
            InitializeComponent();
            boardControlGame.RenderBoard(BoardState.CreateInitial());
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
