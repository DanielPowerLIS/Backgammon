using Backgammon.Server.Services.Data;

namespace Backgammon.Server.Domain
{ 
    internal static class MatchBoardFactory
    {
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

        public static MatchBoardState CreateInitial()
        {
            var boardState = new MatchBoardState();

            PlaceCheckers(
                boardState.Points[BlueBackPoint - 1],
                CheckerColor.Blue, BackCheckerCount);

            PlaceCheckers(
                boardState.Points[BlueMidPoint - 1],
                CheckerColor.Blue, MidCheckerCount);

            PlaceCheckers(
                boardState.Points[BlueOuterPoint - 1],
                CheckerColor.Blue, OuterCheckerCount);

            PlaceCheckers(
                boardState.Points[BlueHomePoint - 1],
                CheckerColor.Blue, HomeCheckerCount);

            PlaceCheckers(
                boardState.Points[PinkBackPoint - 1],
                CheckerColor.Pink, BackCheckerCount);

            PlaceCheckers(
                boardState.Points[PinkMidPoint - 1],
                CheckerColor.Pink, MidCheckerCount);

            PlaceCheckers(
                boardState.Points[PinkOuterPoint - 1],
                CheckerColor.Pink, OuterCheckerCount);

            PlaceCheckers(
                boardState.Points[PinkHomePoint - 1],
                CheckerColor.Pink, HomeCheckerCount);

            return boardState;
        }

        private static void PlaceCheckers(MatchPointState targetPoint, CheckerColor checkerColor, int checkerCount)
        {
            targetPoint.Owner = checkerColor;
            targetPoint.Count = checkerCount;
        }
    }
}