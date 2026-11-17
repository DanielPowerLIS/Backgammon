using System;

using Backgammon.Client.GameReference;

namespace Backgammon.Client.Controllers.Game
{
    public class BoardState
    {
        private static readonly int TotalPoints = 24;

        private static readonly int BlueBackPoint = 24;
        private static readonly int BlueMidPoint = 13;
        private static readonly int BlueOuterPoint = 8;
        private static readonly int BlueHomePoint = 6;

        private static readonly int PinkBackPoint = 1;
        private static readonly int PinkMidPoint = 12;
        private static readonly int PinkOuterPoint = 17;
        private static readonly int PinkHomePoint = 19;

        private static readonly int BackCheckerCount = 2;
        private static readonly int MidCheckerCount = 5;
        private static readonly int OuterCheckerCount = 3;
        private static readonly int HomeCheckerCount = 5;

        private readonly BoardPoint[] _points;

        public BoardState() 
        {
            _points = new BoardPoint[TotalPoints];
            for (int pointIndex = 0; pointIndex < TotalPoints; pointIndex++)
            {
                _points[pointIndex] = new BoardPoint(pointIndex + 1);
            }
        }

        public int BlueOnBar { get; set; }
        public int PinkOnBar { get; set; }
        public int BlueBorneOff { get; set; }
        public int PinkBorneOff { get; set; }

        public static BoardState CreateInitial() 
        {
            var initialState = new BoardState();
            initialState.PlaceCheckers(BlueBackPoint, CheckerColor.Blue, BackCheckerCount);
            initialState.PlaceCheckers(BlueMidPoint, CheckerColor.Blue, MidCheckerCount);
            initialState.PlaceCheckers(BlueOuterPoint, CheckerColor.Blue, OuterCheckerCount);
            initialState.PlaceCheckers(BlueHomePoint, CheckerColor.Blue, HomeCheckerCount);

            initialState.PlaceCheckers(PinkBackPoint, CheckerColor.Pink, BackCheckerCount);
            initialState.PlaceCheckers(PinkMidPoint, CheckerColor.Pink, MidCheckerCount);
            initialState.PlaceCheckers(PinkOuterPoint, CheckerColor.Pink, OuterCheckerCount);
            initialState.PlaceCheckers(PinkHomePoint, CheckerColor.Pink, HomeCheckerCount);

            return initialState;
        }

        public BoardPoint GetPoint(int pointNumber)
        {
            if (pointNumber < 1 || pointNumber > TotalPoints)
            {
                throw new ArgumentOutOfRangeException(nameof(pointNumber));
            }
            return _points[pointNumber - 1];
        }

        public static BoardState CreateFromMatchBoard(MatchBoardState matchBoard)
        {
            if (matchBoard == null)
            {
                throw new ArgumentNullException(nameof(matchBoard));
            }

            if (matchBoard.Points == null || matchBoard.Points.Length != TotalPoints)
            {
                throw new ArgumentException("The board must contain all 24 points.", nameof(matchBoard));
            }

            var boardState = new BoardState
            {
                BlueOnBar = matchBoard.BlueOnBar,
                PinkOnBar = matchBoard.PinkOnBar,
                BlueBorneOff = matchBoard.BlueBorneOff,
                PinkBorneOff = matchBoard.PinkBorneOff
            };

            foreach (MatchPointState sourcePoint in matchBoard.Points)
            {
                if (sourcePoint == null)
                {
                    throw new ArgumentException("The board contains an unavailable point.", nameof(matchBoard));
                }

                BoardPoint targetPoint = boardState.GetPoint(sourcePoint.Number);

                targetPoint.Owner = sourcePoint.Owner;
                targetPoint.Count = sourcePoint.Count;
            }

            return boardState;
        }

        private void PlaceCheckers(int pointNumber, CheckerColor checkerColor, int checkerCount)
        {
            BoardPoint targetPoint = GetPoint(pointNumber);
            targetPoint.Owner = checkerColor;
            targetPoint.Count = checkerCount;
        }
    }
}
