using System;
using System.Collections.Generic;

using Backgammon.Server.Services.Data;

namespace Backgammon.Server.Domain
{
    internal sealed class MatchBearingOff
    {
        private const int MinimumPointNumber = 1;
        private const int MaximumPointNumber = 24;
        private const int TotalCheckers = 15;
        private const int NoCheckers = 0;

        public const int BorneOffDestinationPointNumber = 26;

        private const int FirstDieIndex = 1;
        private const int SecondDieIndex = 2;
        private const int SingleDieUse = 1;
        private const int NoRemainingUses = 0;
        private const int SingleCheckerCount = 1;

        private readonly MatchBoardState _boardState;
        private readonly CheckerColor _playerColor;
        private readonly int _finalPointNumber;

        public MatchBearingOff(MatchBoardState boardState, CheckerColor playerColor)
        {
            if (boardState == null)
            {
                throw new ArgumentNullException(nameof(boardState));
            }

            if (boardState.Points == null || boardState.Points.Length != MaximumPointNumber)
            {
                throw new ArgumentException("The board must contain all expected points.", nameof(boardState));
            }

            if (playerColor != CheckerColor.Blue && playerColor != CheckerColor.Pink)
            {
                throw new ArgumentOutOfRangeException(nameof(playerColor));
            }

            _boardState = boardState;
            _playerColor = playerColor;
            _finalPointNumber = MaximumPointNumber;

            if (playerColor == CheckerColor.Blue)
            {
                _finalPointNumber = MinimumPointNumber;
            }
        }

        public bool CanBearOff()
        {
            MatchPointState finalPoint = GetFinalPoint();

            int capturedCheckers = GetCapturedCount();
            int borneOffCheckers = GetBorneOffCount();

            bool canBearOff = capturedCheckers == NoCheckers &&
                finalPoint.Owner == _playerColor &&
                finalPoint.Count > NoCheckers &&
                finalPoint.Count + borneOffCheckers == TotalCheckers;

            return canBearOff;
        }

        public CheckerMove[] GetAvailableMoves(MatchDiceState diceState)
        {
            if (diceState == null)
            {
                throw new ArgumentNullException(nameof(diceState));
            }

            var availableMoves = new List<CheckerMove>();

            if (CanBearOff())
            {
                if (diceState.FirstDieRemainingUses > NoRemainingUses)
                {
                    CheckerMove firstDieMove = CreateMove(FirstDieIndex);

                    availableMoves.Add(firstDieMove);
                }

                if (diceState.SecondDieRemainingUses > NoRemainingUses)
                {
                    CheckerMove secondDieMove = CreateMove(SecondDieIndex);

                    availableMoves.Add(secondDieMove);
                }
            }

            CheckerMove[] moves = availableMoves.ToArray();

            return moves;
        }

        public void RemoveChecker()
        {
            if (!CanBearOff())
            {
                throw new ArgumentException("The player cannot remove a checker from the board.");
            }

            MatchPointState finalPoint = GetFinalPoint();

            finalPoint.Count -= SingleCheckerCount;

            if (finalPoint.Count == NoCheckers)
            {
                finalPoint.Owner = CheckerColor.None;
            }

            if (_playerColor == CheckerColor.Blue)
            {
                _boardState.BlueBorneOff += SingleCheckerCount;
            }
            else
            {
                _boardState.PinkBorneOff += SingleCheckerCount;
            }
        }

        public bool HasWon()
        {
            int borneOffCheckers = GetBorneOffCount();

            bool hasWon = borneOffCheckers == TotalCheckers;

            return hasWon;
        }

        private MatchPointState GetFinalPoint()
        {
            int pointIndex = _finalPointNumber - MinimumPointNumber;

            MatchPointState finalPoint = _boardState.Points[pointIndex];

            return finalPoint;
        }

        private int GetCapturedCount()
        {
            int capturedCheckers = _boardState.PinkOnBar;

            if (_playerColor == CheckerColor.Blue)
            {
                capturedCheckers = _boardState.BlueOnBar;
            }

            return capturedCheckers;
        }

        private int GetBorneOffCount()
        {
            int borneOffCheckers = _boardState.PinkBorneOff;

            if (_playerColor == CheckerColor.Blue)
            {
                borneOffCheckers = _boardState.BlueBorneOff;
            }

            return borneOffCheckers;
        }

        private CheckerMove CreateMove(int dieIndex)
        {
            var move = new CheckerMove
            {
                SourcePointNumber = _finalPointNumber,
                DestinationPointNumber = BorneOffDestinationPointNumber
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
    }
}