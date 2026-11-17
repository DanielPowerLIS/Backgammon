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
                Version = matchState.Version
            };

            return stateCopy;
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

    }
}