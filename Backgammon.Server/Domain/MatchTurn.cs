using System;

using Backgammon.Server.Services.Data;

namespace Backgammon.Server.Domain
{
    internal static class MatchTurn
    {
        private static readonly int SingleDieUses = 1;
        private static readonly int DoubleDieUses = 2;

        public static void RollDice(MatchState matchState, int playerId, CheckerColor playerColor)
        {
            if (matchState == null)
            {
                throw new ArgumentNullException(nameof(matchState));
            }

            ValidateRoll(matchState, playerId);

            MatchDiceState diceState = CreateDice();

            var movement = new MatchMovement(matchState.Board, diceState, playerColor);
            CheckerMove[] availableMoves = movement.GetAvailableMoves();

            matchState.Dice = diceState;
            matchState.AvailableMoves = availableMoves;
            matchState.Phase = MatchPhase.MovingCheckers;
        }

        private static void ValidateRoll(MatchState matchState, int playerId)
        {
            if (matchState.CurrentPlayerId != playerId)
            {
                throw new UnauthorizedAccessException("The player cannot roll outside their turn.");
            }

            if (matchState.Phase != MatchPhase.WaitingForRoll)
            {
                throw new ArgumentException("The match is not waiting for a turn roll.");
            }
        }

        private static MatchDiceState CreateDice()
        {
            int firstDieValue = Die.Roll();
            int secondDieValue = Die.Roll();
            int remainingUses = SingleDieUses;

            if (firstDieValue == secondDieValue)
            {
                remainingUses = DoubleDieUses;
            }

            var diceState = new MatchDiceState
            {
                FirstDieValue = firstDieValue,
                SecondDieValue = secondDieValue,
                FirstDieRemainingUses = remainingUses,
                SecondDieRemainingUses = remainingUses
            };

            return diceState;
        }
    }
}