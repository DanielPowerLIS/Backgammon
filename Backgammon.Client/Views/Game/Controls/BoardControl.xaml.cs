using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Backgammon.Client.Models.Game;

namespace Backgammon.Client.Views.Game.Controls
{
    public partial class BoardControl : UserControl
    {
        private const double BoardTop = 30;
        private const double BoardBottom = 415;
        private const double PointWidth = 50;
        private const double PointBaseLeft = 40;
        private const double BarWidth = 100;
        private const double CheckerSize = 40;
        private const double CheckerMarginFromEdge = 5;
        private const double StackAreaHeight = 140;
        private const double CheckerOffserWithinPoint = (PointWidth - CheckerSize)/2;
        private const int MaxCheckersWithoutOverlap = 3;
        private const int TopRowMinPointNumber = 13;
        private const int TotalColumns = 12;
        private const int ColumnsPerHalf = 6;
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
                BoardCanvas.Children.Remove(renderedChecker);
            }

            _renderedCheckers.Clear();
        }

        private void RenderPointCheckers(BoardPoint boardPoint)
        {
            if(boardPoint.Count == 0)
            {
                return;
            }

            double checkerSpacing = CalculateCheckerSpacing(boardPoint.Count);
            int columnIndex = GetColumnIndex(boardPoint.Number);
            double checkerLeft = CalculateCheckerLeft(columnIndex);

            for(int stackIndex = 0; stackIndex < boardPoint.Count; stackIndex++) 
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

            if(columnIndex >= ColumnsPerHalf)
            {
                pointLeft += BarWidth;
            }

            return pointLeft + CheckerOffserWithinPoint;
        }

        private static double CalculateCheckerTop(int pointNumber, int stackIndex, double checkerSpacing) 
        {
            if(pointNumber >= TopRowMinPointNumber)
            {
                return BoardTop  - CheckerMarginFromEdge + stackIndex * checkerSpacing;
            }

            return BoardBottom - CheckerMarginFromEdge - CheckerSize - stackIndex * checkerSpacing;
        }

        private void AddCheckerToCanvas(PlayerColor checkerColor, double checkerLeft, double checkerTop) 
        {
            var newChecker = new CheckerControl { CheckerColor = checkerColor};

            Canvas.SetLeft(newChecker, checkerLeft);
            Canvas.SetTop(newChecker, checkerTop);
            BoardCanvas.Children.Add(newChecker);
            _renderedCheckers.Add(newChecker);
        }
    }
}
