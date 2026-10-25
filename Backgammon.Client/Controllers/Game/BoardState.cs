using System;

namespace Backgammon.Client.Controllers.Game
{
    public class BoardState
    {
        private static readonly int TotalPoints = 24;

        private static readonly int WhiteBackPoint = 24;
        private static readonly int WhiteMidPoint = 13;
        private static readonly int WhiteOuterPoint = 8;
        private static readonly int WhiteHomePoint = 6;

        private static readonly int BlackBackPoint = 1;
        private static readonly int BlackMidPoint = 12;
        private static readonly int BlackOuterPoint = 17;
        private static readonly int BlackHomePoint = 19;

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

        public int WhiteOnBar { get; set; }
        public int BlackOnBar { get; set; }
        public int WhiteBorneOff { get; set; }
        public int BlackBorneOff { get; set; }

        public static BoardState CreateInitial() 
        {
            var initialState = new BoardState();
            initialState.PlaceCheckers(WhiteBackPoint, PlayerColor.White, BackCheckerCount);
            initialState.PlaceCheckers(WhiteMidPoint, PlayerColor.White, MidCheckerCount);
            initialState.PlaceCheckers(WhiteOuterPoint, PlayerColor.White, OuterCheckerCount);
            initialState.PlaceCheckers(WhiteHomePoint, PlayerColor.White, HomeCheckerCount);

            initialState.PlaceCheckers(BlackBackPoint, PlayerColor.Black, BackCheckerCount);
            initialState.PlaceCheckers(BlackMidPoint, PlayerColor.Black, MidCheckerCount);
            initialState.PlaceCheckers(BlackOuterPoint, PlayerColor.Black, OuterCheckerCount);
            initialState.PlaceCheckers(BlackHomePoint, PlayerColor.Black, HomeCheckerCount);

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

        private void PlaceCheckers(int pointNumber, PlayerColor checkerColor, int checkerCount)
        {
            BoardPoint targetPoint = GetPoint(pointNumber);
            targetPoint.Owner = checkerColor;
            targetPoint.Count = checkerCount;
        }
    }
}
