using System;

using Backgammon.Server.Services.Data;

namespace Backgammon.Server.Domain
{
    internal sealed class TurnSnapshot
    {
        public MatchBoardState Board { get; }

        public MatchDiceState Dice { get; }

        public int SharedPot { get; }

        public TurnSnapshot(MatchBoardState board, MatchDiceState dice, int sharedPot)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (dice == null)
            {
                throw new ArgumentNullException(nameof(dice));
            }

            Board = board;
            Dice = dice;
            SharedPot = sharedPot;
        }
    }
}