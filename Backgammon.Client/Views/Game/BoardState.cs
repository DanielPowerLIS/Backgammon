using System;

namespace Backgammon.Client.Views.Game
{
    public class BoardState
    {
        private const int TotalPoints = 24;

        private const int WhiteBackPoint = 24;
        private const int WhiteMidPoint = 13;
        private const int WhiteOuterPoint = 8;
        private const int WhiteHomePoint = 6;

        private const int BlackBackPoint = 1;
        private const int BlackMidPoint = 12;
        private const int BlackOuterPoint = 17;
        private const int BlackHomePoint = 19;

        private const int BackCheckerPoint = 2;
        private const int MidCheckerPoint = 5;
        private const int OuterCheckerPoint = 3;
        private const int HomeCheckerPoint = 5;

        private readonly BoardPoint[] _points;

        public BoardState() 
        {
            _points = new BoardPoint[TotalPoints];
            for(int pointIndex = 0; pointIndex < TotalPoints; pointIndex++)
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
            initialState.PlaceCheckers(WhiteBackPoint, PlayerColor.White, BackCheckerPoint);
            initialState.PlaceCheckers(WhiteMidPoint, PlayerColor.White, MidCheckerPoint);
            initialState.PlaceCheckers(WhiteOuterPoint, PlayerColor.White, OuterCheckerPoint);
            initialState.PlaceCheckers(WhiteHomePoint, PlayerColor.White, HomeCheckerPoint);

            initialState.PlaceCheckers(BlackBackPoint, PlayerColor.Black, BackCheckerPoint);
            initialState.PlaceCheckers(BlackMidPoint, PlayerColor.Black, MidCheckerPoint);
            initialState.PlaceCheckers(BlackOuterPoint, PlayerColor.Black, OuterCheckerPoint);
            initialState.PlaceCheckers(BlackHomePoint, PlayerColor.Black, HomeCheckerPoint);

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
