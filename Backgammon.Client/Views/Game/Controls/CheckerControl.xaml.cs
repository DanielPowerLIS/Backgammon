using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Backgammon.Client.Controllers.Game;

namespace Backgammon.Client.Views.Game.Controls
{
    public partial class CheckerControl : UserControl
    {
        public static readonly DependencyProperty CheckerColorProperty =
            DependencyProperty.Register(nameof(CheckerColor), typeof(PlayerColor),
                typeof(CheckerControl), new PropertyMetadata(PlayerColor.None, OnCheckerColorChanged));
        private static readonly string BlueCheckerHex = "#00C9F2";
        private static readonly string PinkCheckerHex = "#E795EC";
        private static readonly string NoneCheckerHex = "#4A5366";
        private static readonly string BlueCheckerStrokeHex = "#E795EC";
        private static readonly string PinkCheckerStrokeHex = "#00C9F2";
        private static readonly string NoneCheckerStrokeHex = "#00C9F2";

        public CheckerControl()
        {
            InitializeComponent();
        }

        public PlayerColor CheckerColor
        {
            get
            {
                return (PlayerColor)GetValue(CheckerColorProperty);
            }
            set
            {
                SetValue(CheckerColorProperty, value);
            }
        }

        private static void OnCheckerColorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            CheckerControl changedChecker = (CheckerControl)sender;

            changedChecker.ApplyCheckerColor((PlayerColor)e.NewValue);
        }

        private void ApplyCheckerColor(PlayerColor checkerColor)
        {
            string fillHex = GetFillHex(checkerColor);
            string strokeHex = GetStrokeHex(checkerColor);

            ellipseChecker.Fill = (Brush)new BrushConverter().ConvertFromString(fillHex);
            ellipseChecker.Stroke = (Brush)new BrushConverter().ConvertFromString(strokeHex);
        }

        private static string GetFillHex(PlayerColor checkerColor)
        {
            switch (checkerColor)
            {
                case PlayerColor.Blue:
                    return BlueCheckerHex;
                case PlayerColor.Pink:
                    return PinkCheckerHex;
                default:
                    return NoneCheckerHex;
            }
        }

        private static string GetStrokeHex(PlayerColor checkerColor)
        {
            switch (checkerColor)
            {
                case PlayerColor.Blue:
                    return BlueCheckerStrokeHex;
                case PlayerColor.Pink:
                    return PinkCheckerStrokeHex;
                default:
                    return NoneCheckerStrokeHex;
            }
        }

    }
}
