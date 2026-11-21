using System;

using Backgammon.Server.Services.Data;

namespace Backgammon.Server.Domain
{
    internal static class MatchTurn
    {
        private static readonly int SingleDieUses = 1;
        private static readonly int DoubleDieUses = 2;
        private static readonly int CapturePoints = 10;

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

            matchState.Turn.Dice = diceState;
            matchState.Turn.AvailableMoves = availableMoves;
            matchState.Phase = MatchPhase.MovingCheckers;
        }

        public static void ValidatePlayerTurn(MatchState matchState, int playerId)
        {
            if (matchState == null)
            {
                throw new ArgumentNullException(nameof(matchState));
            }

            if (matchState.Turn.CurrentPlayerId != playerId)
            {
                throw new UnauthorizedAccessException("The player cannot act outside their turn.");
            }
        }

        public static void MoveChecker(MatchState matchState, CheckerColor playerColor, CheckerMove selectedMove)
        {
            if (matchState == null)
            {
                throw new ArgumentNullException(nameof(matchState));
            }

            if (matchState.Phase != MatchPhase.MovingCheckers)
            {
                throw new ArgumentException("The match is not accepting checker movements.");
            }

            var movement = new MatchMovement(
                matchState.Board,
                matchState.Turn.Dice,
                playerColor);

            bool checkerCaptured = movement.MoveChecker(selectedMove);

            if (checkerCaptured)
            {
                matchState.SharedPot += CapturePoints;
                matchState.CaptureNumber++;
            }

            CheckerMove[] availableMoves = movement.GetAvailableMoves();

            matchState.Turn.AvailableMoves = availableMoves;
        }

        private static void ValidateRoll(MatchState matchState, int playerId)
        {
            ValidatePlayerTurn(matchState, playerId);

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