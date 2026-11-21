using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Backgammon.Client.GameReference;

namespace Backgammon.Client.Views.Game.Controls
{
    public partial class CheckerControl : UserControl
    {
        public static readonly DependencyProperty CheckerColorProperty =
            DependencyProperty.Register(nameof(CheckerColor), typeof(CheckerColor),
                typeof(CheckerControl), new PropertyMetadata(CheckerColor.None, OnCheckerColorChanged));
        private static readonly string BlueCheckerHex = "#00C9F2";
        private static readonly string PinkCheckerHex = "#E795EC";
        private static readonly string NoneCheckerHex = "#4A5366";
        private static readonly string BlueCheckerStrokeHex = "#E795EC";
        private static readonly string PinkCheckerStrokeHex = "#00C9F2";
        private static readonly string NoneCheckerStrokeHex = "#00C9F2";

        private const int UnassignedPointNumber = -1;
        private static readonly double NormalStrokeThickness = 2;
        private static readonly double HighlightedStrokeThickness = 4;

        public CheckerControl()
        {
            InitializeComponent();

            PointNumber = UnassignedPointNumber;
            IsHighlighted = false;
        }

        public int PointNumber { get; set; }

        public bool IsHighlighted { get; private set; }

        public CheckerColor CheckerColor
        {
            get
            {
                return (CheckerColor)GetValue(CheckerColorProperty);
            }
            set
            {
                SetValue(CheckerColorProperty, value);
            }
        }

        public void SetHighlighted(bool isHighlighted)
        {
            IsHighlighted = isHighlighted;

            string strokeHex = GetStrokeHex(CheckerColor);

            Brush strokeBrush = (Brush)new BrushConverter().ConvertFromString(strokeHex);

            double strokeThickness = NormalStrokeThickness;

            if (isHighlighted)
            {
                strokeBrush = Brushes.White;
                strokeThickness = HighlightedStrokeThickness;
            }

            ellipseChecker.Stroke = strokeBrush;
            ellipseChecker.StrokeThickness = strokeThickness;
        }

        private static void OnCheckerColorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            CheckerControl changedChecker = (CheckerControl)sender;

            changedChecker.ApplyCheckerColor((CheckerColor)e.NewValue);
        }

        private void ApplyCheckerColor(CheckerColor checkerColor)
        {
            string fillHex = GetFillHex(checkerColor);

            ellipseChecker.Fill = (Brush)new BrushConverter().ConvertFromString(fillHex);

            SetHighlighted(IsHighlighted);
        }

        private static string GetFillHex(CheckerColor checkerColor)
        {
            switch (checkerColor)
            {
                case CheckerColor.Blue:
                    return BlueCheckerHex;
                case CheckerColor.Pink:
                    return PinkCheckerHex;
                default:
                    return NoneCheckerHex;
            }
        }

        private static string GetStrokeHex(CheckerColor checkerColor)
        {
            switch (checkerColor)
            {
                case CheckerColor.Blue:
                    return BlueCheckerStrokeHex;
                case CheckerColor.Pink:
                    return PinkCheckerStrokeHex;
                default:
                    return NoneCheckerStrokeHex;
            }
        }

    }
}
