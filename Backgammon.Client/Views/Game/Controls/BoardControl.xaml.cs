using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

using Backgammon.Client.Controllers.Game;
using Backgammon.Client.GameReference;

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
        private static readonly int FirstPointIndex = 0;

        private static readonly int MinimumPointNumber = 1;
        private static readonly int MaximumPointNumber = 24;
        private static readonly int PinkBarPointNumber = 0;
        private static readonly int BlueBarPointNumber = 25;
        private static readonly int FirstCheckerIndex = 0;
        private static readonly double CenteringDivisor = 2;
        private static readonly string DestinationHighlightHex = "#6600C9F2";

        private readonly List<Polygon> _pointPolygons;
        private readonly HashSet<int> _destinationPoints;
        private readonly Brush _destinationBrush;
        private readonly List<CheckerControl> _renderedCheckers = new List<CheckerControl>();

        public BoardControl()
        {
            InitializeComponent();

            _pointPolygons = new List<Polygon>();
            _destinationPoints = new HashSet<int>();

            _destinationBrush = (Brush)new BrushConverter().ConvertFromString(
                DestinationHighlightHex);

            RegisterPoints();

            HighlightDestinations(Array.Empty<int>());
        }

        public event EventHandler<BoardPointSelection> CheckerSelected;

        public event EventHandler<BoardPointSelection> DestinationSelected;

        public void RenderBoard(BoardState boardState)
        {
            HighlightDestinations(Array.Empty<int>());

            ClearRenderedCheckers();

            for (int pointNumber = MinimumPointNumber;
                pointNumber <= MaximumPointNumber;
                pointNumber++)
            {
                RenderPointCheckers(boardState.GetPoint(pointNumber));
            }

            RenderBarCheckers(BlueBarPointNumber, CheckerColor.Blue, boardState.BlueOnBar);

            RenderBarCheckers(PinkBarPointNumber, CheckerColor.Pink, boardState.PinkOnBar);
        }

        public void HighlightCheckers(IEnumerable<int> sourcePointNumbers)
        {
            if (sourcePointNumbers == null)
            {
                throw new ArgumentNullException(nameof(sourcePointNumbers));
            }

            var availablePoints = new HashSet<int>(sourcePointNumbers);

            foreach (CheckerControl checker in _renderedCheckers)
            {
                bool isHighlighted = availablePoints.Contains(checker.PointNumber);

                checker.SetHighlighted(isHighlighted);
            }
        }

        public void HighlightDestinations(IEnumerable<int> destinationPointNumbers)
        {
            if (destinationPointNumbers == null)
            {
                throw new ArgumentNullException(nameof(destinationPointNumbers));
            }

            _destinationPoints.Clear();
            _destinationPoints.UnionWith(destinationPointNumbers);

            for (int pointIndex = FirstPointIndex; pointIndex < _pointPolygons.Count; pointIndex++)
            {
                Polygon pointPolygon = _pointPolygons[pointIndex];

                int pointNumber = GetPointNumber(pointIndex);

                Brush pointBrush = Brushes.Transparent;

                if (_destinationPoints.Contains(pointNumber))
                {
                    pointBrush = _destinationBrush;
                }

                pointPolygon.Fill = pointBrush;
            }
        }

        private void ClearRenderedCheckers()
        {
            foreach (CheckerControl renderedChecker in _renderedCheckers)
            {
                renderedChecker.MouseLeftButtonUp -= OnCheckerMouseLeftButtonUp;
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
                AddCheckerToCanvas(boardPoint, checkerLeft, checkerTop);
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

        private void AddCheckerToCanvas(BoardPoint boardPoint, double checkerLeft, double checkerTop)
        {
            var checker = new CheckerControl
            {
                PointNumber = boardPoint.Number,
                CheckerColor = boardPoint.Owner
            };

            AddCheckerToCanvas(checker, checkerLeft, checkerTop);
        }

        private void AddCheckerToCanvas(CheckerControl checker, double checkerLeft, double checkerTop)
        {
            checker.MouseLeftButtonUp += OnCheckerMouseLeftButtonUp;

            Canvas.SetLeft(checker, checkerLeft);
            Canvas.SetTop(checker, checkerTop);

            CanvasBoard.Children.Add(checker);
            _renderedCheckers.Add(checker);
        }

        private void OnCheckerMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseEventArgs)
        {
            CheckerControl checker = (CheckerControl)sender;

            if (_destinationPoints.Contains(checker.PointNumber))
            {
                mouseEventArgs.Handled = true;

                SelectDestination(checker.PointNumber);

                return;
            }

            if (!checker.IsHighlighted)
            {
                return;
            }

            mouseEventArgs.Handled = true;

            var selection = new BoardPointSelection(checker.PointNumber);

            EventHandler<BoardPointSelection> selectionHandler = CheckerSelected;

            if (selectionHandler != null)
            {
                selectionHandler.Invoke(this, selection);
            }
        }

        private void RegisterPoints()
        {
            foreach (UIElement child in CanvasBoard.Children)
            {
                if (child is Polygon pointPolygon)
                {
                    _pointPolygons.Add(pointPolygon);

                    pointPolygon.MouseLeftButtonUp += OnPointMouseLeftButtonUp;
                }
            }
            
        }

        private static int GetPointNumber(int pointIndex)
        {
            int pointNumber = TopRowMinPointNumber + pointIndex;

            if (pointIndex >= TotalColumns)
            {
                int bottomColumnIndex = pointIndex - TotalColumns;

                pointNumber = TotalColumns - bottomColumnIndex;
            }

            return pointNumber;
        }

        private void OnPointMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseEventArgs)
        {
            Polygon pointPolygon = (Polygon)sender;

            int pointIndex = _pointPolygons.IndexOf(pointPolygon);

            if (pointIndex < FirstPointIndex)
            {
                return;
            }

            int pointNumber = GetPointNumber(pointIndex);

            if (!_destinationPoints.Contains(pointNumber))
            {
                return;
            }

            mouseEventArgs.Handled = true;

            SelectDestination(pointNumber);
        }

        private void SelectDestination(int pointNumber)
        {
            var selection = new BoardPointSelection(pointNumber);

            EventHandler<BoardPointSelection> selectionHandler = DestinationSelected;

            if (selectionHandler != null)
            {
                selectionHandler.Invoke(this, selection);
            }
        }

        private void RenderBarCheckers(int barPointNumber, CheckerColor checkerColor, int checkerCount)
        {
            double checkerSpacing = CalculateCheckerSpacing(checkerCount);
            double checkerLeft = CalculateBarCheckerLeft();

            for (int stackIndex = FirstCheckerIndex;
                stackIndex < checkerCount;
                stackIndex++)
            {
                double checkerTop = CalculateCheckerTop(
                    barPointNumber,
                    stackIndex,
                    checkerSpacing);

                var checker = new CheckerControl
                {
                    PointNumber = barPointNumber,
                    CheckerColor = checkerColor
                };

                AddCheckerToCanvas(checker, checkerLeft, checkerTop);
            }
        }

        private static double CalculateBarCheckerLeft()
        {
            double barLeft = PointBaseLeft + ColumnsPerHalf * PointWidth;

            double checkerLeft = barLeft + (BarWidth - CheckerSize) / CenteringDivisor;

            return checkerLeft;
        }
    }
}
