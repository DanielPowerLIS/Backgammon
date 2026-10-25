using System.Collections.Generic;
using System.Windows.Controls;

using Backgammon.Client.Controllers.Game;

namespace Backgammon.Client.Views.Game.Controls
{
    public partial class BoardControl : UserControl
    {
        private static readonly double BoardTop = 30;
        private static readonly double BoardBottom = 415;
        private static readonly double PointWidth = 50;
        private static readonly double PointBaseLeft = 40;
        private static readonly double BarWidth = 100;  
        private static readonly double CheckerSize = 40;
        private static readonly double CheckerMarginFromEdge = 5;
        private static readonly double StackAreaHeight = 140;
        private static readonly double CheckerOffserWithinPoint = (PointWidth - CheckerSize) / 2;
        private static readonly int MaxCheckersWithoutOverlap = 3;
        private static readonly int TopRowMinPointNumber = 13;
        private static readonly int TotalColumns = 12;
        private static readonly int ColumnsPerHalf = 6;
        private readonly List<CheckerControl> _renderedCheckers = new List<CheckerControl>();

        public BoardControl()
        {
            InitializeComponent();
        }

        public void RenderBoard(BoardState boardState) 
        {
            ClearRenderedCheckers();
            for (int pointNumber = 1; pointNumber <= 24; pointNumber++)
            {
                RenderPointCheckers(boardState.GetPoint(pointNumber));
            }
        }

        private void ClearRenderedCheckers()
        {
            foreach (CheckerControl renderedChecker in _renderedCheckers)
            {
                CanvasBoard.Children.Remove(renderedChecker);
            }

            _renderedCheckers.Clear();
        }

        private void RenderPointCheckers(BoardPoint boardPoint)
        {
            if (boardPoint.Count == 0)
            {
                return;
            }

            double checkerSpacing = CalculateCheckerSpacing(boardPoint.Count);
            int columnIndex = GetColumnIndex(boardPoint.Number);
            double checkerLeft = CalculateCheckerLeft(columnIndex);

            for (int stackIndex = 0; stackIndex < boardPoint.Count; stackIndex++) 
            {
                double checkerTop = CalculateCheckerTop(boardPoint.Number, stackIndex, checkerSpacing);
                AddCheckerToCanvas(boardPoint.Owner, checkerLeft, checkerTop);
            }
        }

        private static double CalculateCheckerSpacing(int checkerCount) 
        {
            if (checkerCount <= MaxCheckersWithoutOverlap) 
            {
                return CheckerSize;
            }

            return (StackAreaHeight - CheckerSize) / (checkerCount - 1);
        }

        private static int GetColumnIndex(int pointNumber) 
        {
            if (pointNumber >= TopRowMinPointNumber) 
            {
                return pointNumber - TopRowMinPointNumber;
            }

            return TotalColumns - pointNumber;
        }

        private static double CalculateCheckerLeft(int columnIndex) 
        {
            double pointLeft = PointBaseLeft + columnIndex * PointWidth;

            if (columnIndex >= ColumnsPerHalf)
            {
                pointLeft += BarWidth;
            }

            return pointLeft + CheckerOffserWithinPoint;
        }

        private static double CalculateCheckerTop(int pointNumber, int stackIndex, double checkerSpacing) 
        {
            if (pointNumber >= TopRowMinPointNumber)
            {
                return BoardTop - CheckerMarginFromEdge + stackIndex * checkerSpacing;
            }

            return BoardBottom - CheckerMarginFromEdge - CheckerSize - stackIndex * checkerSpacing;
        }

        private void AddCheckerToCanvas(PlayerColor checkerColor, double checkerLeft, double checkerTop) 
        {
            var newChecker = new CheckerControl { CheckerColor = checkerColor};

            Canvas.SetLeft(newChecker, checkerLeft);
            Canvas.SetTop(newChecker, checkerTop);
            CanvasBoard.Children.Add(newChecker);
            _renderedCheckers.Add(newChecker);
        }
    }
}
