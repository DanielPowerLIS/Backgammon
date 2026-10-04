using System;
using Backgammon.Client.Models.Game;

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

        private const int BackCheckerCount = 2;
        private const int MidCheckerCount = 5;
        private const int OuterCheckerCount = 3;
        private const int HomeCheckerCount = 5;

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
