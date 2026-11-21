using System;

using Backgammon.Server.Services.Data;

namespace Backgammon.Server.Domain
{
    internal static class MatchStateFactory
    {
        private const long InitialVersion = 1;

        public static MatchState CreateInitial(MatchSession matchSession)
        {
            if (matchSession == null)
            {
                throw new ArgumentNullException(nameof(matchSession));
            }

            var matchState = new MatchState
            {
                MatchId = matchSession.MatchId,
                RoomId = matchSession.RoomId,
                Phase = MatchPhase.WaitingForPlayers,
                Board = MatchBoardFactory.CreateInitial(),
                Version = InitialVersion
            };

            return matchState;
        }

        public static MatchState Copy(MatchState matchState)
        {
            if (matchState == null)
            {
                throw new ArgumentNullException(nameof(matchState));
            }

            var stateCopy = new MatchState
            {
                MatchId = matchState.MatchId,
                RoomId = matchState.RoomId,
                Phase = matchState.Phase,
                Board = CopyBoard(matchState.Board),
                SharedPot = matchState.SharedPot,
                CaptureNumber = matchState.CaptureNumber,
                Version = matchState.Version,
                Turn = CopyTurn(matchState.Turn)
            };

            return stateCopy;
        }

        public static TurnSnapshot CreateTurnSnapshot(MatchState matchState)
        {
            if (matchState == null)
            {
                throw new ArgumentNullException(nameof(matchState));
            }

            if (matchState.Turn == null)
            {
                throw new ArgumentException("The match turn is required.", nameof(matchState));
            }

            MatchBoardState boardCopy = CopyBoard(matchState.Board);
            MatchDiceState diceCopy = CopyDice(matchState.Turn.Dice);

            var snapshot = new TurnSnapshot(
                boardCopy,
                diceCopy,
                matchState.SharedPot);

            return snapshot;
        }

        public static void RestoreTurnSnapshot(MatchState matchState, TurnSnapshot snapshot)
        {
            if (matchState == null)
            {
                throw new ArgumentNullException(nameof(matchState));
            }

            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            if (matchState.Turn == null)
            {
                throw new ArgumentException("The match turn is required.", nameof(matchState));
            }

            MatchBoardState boardCopy = CopyBoard(snapshot.Board);
            MatchDiceState diceCopy = CopyDice(snapshot.Dice);

            matchState.Board = boardCopy;
            matchState.Turn.Dice = diceCopy;
            matchState.SharedPot = snapshot.SharedPot;
        }

        private static MatchBoardState CopyBoard(MatchBoardState boardState)
        {
            if (boardState == null)
            {
                throw new ArgumentNullException(nameof(boardState));
            }

            var boardCopy = new MatchBoardState
            {
                BlueOnBar = boardState.BlueOnBar,
                PinkOnBar = boardState.PinkOnBar,
                BlueBorneOff = boardState.BlueBorneOff,
                PinkBorneOff = boardState.PinkBorneOff
            };

            for (int pointIndex = 0; pointIndex < boardState.Points.Length; pointIndex++)
            {
                MatchPointState sourcePoint = boardState.Points[pointIndex];

                boardCopy.Points[pointIndex] = new MatchPointState
                {
                    Number = sourcePoint.Number,
                    Owner = sourcePoint.Owner,
                    Count = sourcePoint.Count
                };
            }

            return boardCopy;
        }

        private static MatchDiceState CopyDice(MatchDiceState diceState)
        {
            if (diceState == null)
            {
                throw new ArgumentNullException(nameof(diceState));
            }

            var diceCopy = new MatchDiceState
            {
                FirstDieValue = diceState.FirstDieValue,
                SecondDieValue = diceState.SecondDieValue,
                FirstDieRemainingUses = diceState.FirstDieRemainingUses,
                SecondDieRemainingUses = diceState.SecondDieRemainingUses
            };

            return diceCopy;
        }

        private static CheckerMove[] CopyMoves(CheckerMove[] availableMoves)
        {
            if (availableMoves == null)
            {
                throw new ArgumentNullException(nameof(availableMoves));
            }

            CheckerMove[] movesCopy = new CheckerMove[availableMoves.Length];

            for (int moveIndex = 0; moveIndex < availableMoves.Length; moveIndex++)
            {
                CheckerMove sourceMove = availableMoves[moveIndex];

                if (sourceMove == null)
                {
                    throw new ArgumentException("The available moves contain an unavailable move.",
                        nameof(availableMoves));
                }

                movesCopy[moveIndex] = new CheckerMove
                {
                    SourcePointNumber = sourceMove.SourcePointNumber,
                    DestinationPointNumber = sourceMove.DestinationPointNumber,
                    FirstDieUses = sourceMove.FirstDieUses,
                    SecondDieUses = sourceMove.SecondDieUses
                };
            }

            return movesCopy;
        }

        private static MatchTurnState CopyTurn(MatchTurnState turnState)
        {
            if (turnState == null)
            {
                throw new ArgumentNullException(nameof(turnState));
            }

            var turnCopy = new MatchTurnState
            {
                CurrentPlayerId = turnState.CurrentPlayerId,
                Dice = CopyDice(turnState.Dice),
                AvailableMoves = CopyMoves(turnState.AvailableMoves),
                CanUndo = turnState.CanUndo
            };

            return turnCopy;
        }
    }
}