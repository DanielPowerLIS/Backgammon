using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Backgammon.Client.Models.Game;

namespace Backgammon.Client.Views.Game.Controls
{
    public partial class DieControl : UserControl
    {
        private const double DotSize = 14;
        private const double DotMargin = 4;
        private const int GridSize = 3;

        private Brush _currentDotBrush = Brushes.White;

        private static readonly (int Column, int Row)[][] DiceFaceLayouts =
        {
            new[] { (1, 1) },
            new[] { (0, 0), (2, 2) },
            new[] { (0, 0), (1, 1), (2, 2) },
            new[] { (0, 0), (0, 2), (2, 0), (2, 2) },
            new[] { (0, 0), (0, 2), (1, 1), (2, 0), (2, 2) },
            new[] { (0, 0), (0, 1), (0, 2), (2, 0), (2, 1), (2, 2) }
        };

        public static readonly DependencyProperty DieValueProperty =
            DependencyProperty.Register(
                nameof(DieValue),
                typeof(int),
                typeof(DieControl),
                new PropertyMetadata(1, OnDieValueChanged));

        public static readonly DependencyProperty DieColorProperty =
            DependencyProperty.Register(
                nameof(DieColor),
                typeof(PlayerColor),
                typeof(DieControl),
                new PropertyMetadata(PlayerColor.None, OnDieColorChanged));

        public DieControl()
        {
            InitializeComponent();
            SetupDotGrid();
        }

        public int DieValue
        {
            get { return (int)GetValue(DieValueProperty); }
            set { SetValue(DieValueProperty, value); }
        }

        public PlayerColor DieColor
        {
            get { return (PlayerColor)GetValue(DieColorProperty); }
            set { SetValue(DieColorProperty, value); }
        }

        private static void OnDieValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            DieControl changedDie = (DieControl)sender;
            changedDie.DrawDots((int)e.NewValue);
        }

        private static void OnDieColorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            DieControl changedDie = (DieControl)sender;
            changedDie.ApplyDieColor((PlayerColor)e.NewValue);
        }

        private void SetupDotGrid()
        {
            for (int rowIndex = 0; rowIndex < GridSize; rowIndex++)
            {
                GridDots.RowDefinitions.Add(new RowDefinition());
            }

            for (int columnIndex = 0; columnIndex < GridSize; columnIndex++)
            {
                GridDots.ColumnDefinitions.Add(new ColumnDefinition());
            }

            CreateAllDots();
        }

        private void CreateAllDots()
        {
            for (int rowIndex = 0; rowIndex < GridSize; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < GridSize; columnIndex++)
                {
                    var dot = new Ellipse
                    {
                        Width = DotSize,
                        Height = DotSize,
                        Margin = new Thickness(DotMargin),
                        Fill = _currentDotBrush,
                        Visibility = Visibility.Hidden,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    Grid.SetColumn(dot, columnIndex);
                    Grid.SetRow(dot, rowIndex);
                    GridDots.Children.Add(dot);
                }
            }
        }

        private void DrawDots(int dieValue)
        {
            foreach (Ellipse dot in GridDots.Children)
            {
                dot.Visibility = Visibility.Hidden;
            }

            (int Column, int Row)[] activePositions = DiceFaceLayouts[dieValue - 1];

            foreach ((int column, int row) in activePositions)
            {
                Ellipse activeDot = GetDotAt(column, row);
                activeDot.Visibility = Visibility.Visible;
            }
        }

        private Ellipse GetDotAt(int column, int row)
        {
            int dotIndex = row * GridSize + column;
            return (Ellipse)GridDots.Children[dotIndex];
        }

        private void ApplyDieColor(PlayerColor dieColor)
        {
            string colorHex = GetColorHex(dieColor);
            _currentDotBrush = (Brush)new BrushConverter().ConvertFromString(colorHex);

            BorderDie.BorderBrush = _currentDotBrush;

            foreach (Ellipse dot in GridDots.Children)
            {
                dot.Fill = _currentDotBrush;
            }

            DrawDots(DieValue);
        }

        private static string GetColorHex(PlayerColor dieColor)
        {
            switch (dieColor)
            {
                case PlayerColor.White:
                    return "#00C9F2";
                case PlayerColor.Black:
                    return "#E795EC";
                default:
                    return "#FFFFFF";
            }
        }
    }
}