using System;

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
            initialState.PlaceCheckers(BlueBackPoint, PlayerColor.Blue, BackCheckerCount);
            initialState.PlaceCheckers(BlueMidPoint, PlayerColor.Blue, MidCheckerCount);
            initialState.PlaceCheckers(BlueOuterPoint, PlayerColor.Blue, OuterCheckerCount);
            initialState.PlaceCheckers(BlueHomePoint, PlayerColor.Blue, HomeCheckerCount);

            initialState.PlaceCheckers(PinkBackPoint, PlayerColor.Pink, BackCheckerCount);
            initialState.PlaceCheckers(PinkMidPoint, PlayerColor.Pink, MidCheckerCount);
            initialState.PlaceCheckers(PinkOuterPoint, PlayerColor.Pink, OuterCheckerCount);
            initialState.PlaceCheckers(PinkHomePoint, PlayerColor.Pink, HomeCheckerCount);

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
