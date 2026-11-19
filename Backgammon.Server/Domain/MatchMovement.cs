using System;
using System.Collections.Generic;

using Backgammon.Server.Services.Data;

namespace Backgammon.Server.Domain
{
    internal class MatchMovement
    {
        private static readonly int MinimumPointNumber = 1;
        private static readonly int MaximumPointNumber = 24;
        private static readonly int FirstDieIndex = 1;
        private static readonly int SecondDieIndex = 2;
        private static readonly int BlockedPointCount = 2;
        private static readonly int BlueDirection = -1;
        private static readonly int PinkDirection = 1;
        private static readonly int NoCheckers = 0;
        private static readonly int NoRemainingUses = 0;
        private static readonly int SingleDieUse = 1;
        private static readonly int SingleCheckerCount = 1;

        private readonly MatchBoardState _boardState;
        private readonly MatchDiceState _diceState;
        private readonly CheckerColor _playerColor;
        private readonly int _direction;
  

        public MatchMovement(MatchBoardState boardState, MatchDiceState diceState, CheckerColor playerColor)
        {
            if (boardState == null)
            {
                throw new ArgumentNullException(nameof(boardState));
            }

            if (diceState == null)
            {
                throw new ArgumentNullException(nameof(diceState));
            }

            if (boardState.Points == null ||
                boardState.Points.Length != MaximumPointNumber)
            {
                throw new ArgumentException("The board must contain all expected points.", nameof(boardState));
            }

            if (playerColor != CheckerColor.Blue && playerColor != CheckerColor.Pink)
            {
                throw new ArgumentOutOfRangeException(nameof(playerColor));
            }

            _boardState = boardState;
            _diceState = diceState;
            _playerColor = playerColor;
            _direction = PinkDirection;

            if (playerColor == CheckerColor.Blue)
            {
                _direction = BlueDirection;
            }
        }

        public CheckerMove[] GetAvailableMoves()
        {
            if (HasCapturedCheckers())
            {
                CheckerMove[] emptyMoves = Array.Empty<CheckerMove>();

                return emptyMoves;
            }

            var availableMoves = new List<CheckerMove>();

            if (_diceState.FirstDieRemainingUses > NoRemainingUses)
            {
                AddDieMoves(availableMoves, _diceState.FirstDieValue, FirstDieIndex);
            }

            if (_diceState.SecondDieRemainingUses > NoRemainingUses)
            {
                AddDieMoves(availableMoves, _diceState.SecondDieValue, SecondDieIndex);
            }

            CheckerMove[] moves = availableMoves.ToArray();

            return moves;
        }

        public CheckerMove GetAvailableMove(CheckerMove selectedMove)
        {
            if (selectedMove == null)
            {
                throw new ArgumentNullException(nameof(selectedMove));
            }

            CheckerMove[] availableMoves = GetAvailableMoves();

            foreach (CheckerMove availableMove in availableMoves)
            {
                if (MatchesMove(availableMove, selectedMove))
                {
                    return availableMove;
                }
            }

            throw new ArgumentException("The selected move is not available.", nameof(selectedMove));
        }

        public void MoveChecker(CheckerMove selectedMove)
        {
            CheckerMove availableMove = GetAvailableMove(selectedMove);

            MatchPointState sourcePoint =
                _boardState.Points[availableMove.SourcePointNumber - MinimumPointNumber];

            MatchPointState destinationPoint =
                _boardState.Points[availableMove.DestinationPointNumber - MinimumPointNumber];

            CaptureChecker(destinationPoint);

            sourcePoint.Count--;

            if (sourcePoint.Count == NoCheckers)
            {
                sourcePoint.Owner = CheckerColor.None;
            }

            destinationPoint.Owner = _playerColor;
            destinationPoint.Count++;

            _diceState.FirstDieRemainingUses -= availableMove.FirstDieUses;
            _diceState.SecondDieRemainingUses -= availableMove.SecondDieUses;
        }

        private bool HasCapturedCheckers()
        {
            int capturedCount = _boardState.PinkOnBar;

            if (_playerColor == CheckerColor.Blue)
            {
                capturedCount = _boardState.BlueOnBar;
            }

            bool hasCapturedCheckers = capturedCount > NoCheckers;

            return hasCapturedCheckers;
        }

        private void AddDieMoves(List<CheckerMove> availableMoves, int dieValue, int dieIndex)
        {
            foreach (MatchPointState sourcePoint in _boardState.Points)
            {
                if (CanMoveFrom(sourcePoint, dieValue))
                {
                    CheckerMove move = CreateMove(sourcePoint, dieValue, dieIndex);

                    availableMoves.Add(move);
                }
            }
        }

        private bool CanMoveFrom(MatchPointState sourcePoint, int dieValue)
        {
            bool ownsCheckers = sourcePoint.Count > NoCheckers && sourcePoint.Owner == _playerColor;

            if (!ownsCheckers)
            {
                return ownsCheckers;
            }

            int destinationNumber = sourcePoint.Number + _direction * dieValue;

            bool insideBoard = destinationNumber >= MinimumPointNumber &&
                destinationNumber <= MaximumPointNumber;

            if (!insideBoard)
            {
                return insideBoard;
            }

            MatchPointState destinationPoint = _boardState.Points[destinationNumber - MinimumPointNumber];

            bool destinationAvailable = destinationPoint.Owner == _playerColor ||
                destinationPoint.Count < BlockedPointCount;

            return destinationAvailable;
        }

        private CheckerMove CreateMove(MatchPointState sourcePoint, int dieValue, int dieIndex)
        {
            int destinationNumber = sourcePoint.Number + _direction * dieValue;

            var move = new CheckerMove
            {
                SourcePointNumber = sourcePoint.Number,
                DestinationPointNumber = destinationNumber
            };

            if (dieIndex == FirstDieIndex)
            {
                move.FirstDieUses = SingleDieUse;
            }
            else
            {
                move.SecondDieUses = SingleDieUse;
            }

            return move;
        }

        private static bool MatchesMove(CheckerMove availableMove, CheckerMove selectedMove)
        {
            bool matchesMove =
                availableMove.SourcePointNumber == selectedMove.SourcePointNumber &&
                availableMove.DestinationPointNumber == selectedMove.DestinationPointNumber &&
                availableMove.FirstDieUses == selectedMove.FirstDieUses &&
                availableMove.SecondDieUses == selectedMove.SecondDieUses;

            return matchesMove;
        }

        private void CaptureChecker(MatchPointState destinationPoint)
        {
            bool containsOpponentChecker = destinationPoint.Count == SingleCheckerCount &&
                destinationPoint.Owner != _playerColor;

            if (!containsOpponentChecker)
            {
                return;
            }

            if (destinationPoint.Owner == CheckerColor.Blue)
            {
                _boardState.BlueOnBar++;
            }
            else
            {
                _boardState.PinkOnBar++;
            }

            destinationPoint.Count = NoCheckers;
            destinationPoint.Owner = CheckerColor.None;
        }
    }
}