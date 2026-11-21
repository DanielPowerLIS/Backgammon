using System;

using Backgammon.Server.Services.Data;

namespace Backgammon.Server.Domain
{
    internal static class MatchTurn
    {
        private static readonly int SingleDieUses = 1;
        private static readonly int DoubleDieUses = 2;
        private static readonly int CapturePoints = 10;
        private const int NoAvailableMoves = 0;
        private const int UnassignedPlayerId = 0;

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
            matchState.Turn.CanConfirm = availableMoves.Length == NoAvailableMoves;
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
            matchState.Turn.CanConfirm = availableMoves.Length == NoAvailableMoves;
        }

        public static void UndoCheckerMove(MatchState matchState, TurnSnapshot snapshot, CheckerColor playerColor)
        {
            if (matchState == null)
            {
                throw new ArgumentNullException(nameof(matchState));
            }

            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            if (matchState.Phase != MatchPhase.MovingCheckers)
            {
                throw new ArgumentException("The match is not accepting movement undo.");
            }

            var movement = new MatchMovement(
                snapshot.Board,
                snapshot.Dice,
                playerColor);

            CheckerMove[] availableMoves = movement.GetAvailableMoves();

            MatchStateFactory.RestoreTurnSnapshot(matchState, snapshot);

            matchState.Turn.AvailableMoves = availableMoves;
            matchState.Turn.CanConfirm = availableMoves.Length == NoAvailableMoves;
        }

        public static void ConfirmTurn(MatchState matchState, CheckerColor playerColor, int nextPlayerId)
        {
            if (matchState == null)
            {
                throw new ArgumentNullException(nameof(matchState));
            }

            if (matchState.Phase != MatchPhase.MovingCheckers)
            {
                throw new ArgumentException("The match is not accepting turn confirmation.");
            }

            if (nextPlayerId == UnassignedPlayerId || nextPlayerId == matchState.Turn.CurrentPlayerId)
            {
                throw new ArgumentException("The next player is invalid.", nameof(nextPlayerId));
            }

            var movement = new MatchMovement(
                matchState.Board,
                matchState.Turn.Dice,
                playerColor);

            CheckerMove[] availableMoves = movement.GetAvailableMoves();

            if (availableMoves.Length != NoAvailableMoves)
            {
                throw new ArgumentException("The player still has available checker movements.");
            }

            var nextTurn = new MatchTurnState
            {
                CurrentPlayerId = nextPlayerId
            };

            matchState.Turn = nextTurn;
            matchState.Phase = MatchPhase.WaitingForRoll;
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