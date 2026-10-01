using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Backgammon.Client.Views.Game.Controls
{
    public partial class CheckerControl : UserControl
    {
        private const string WhiteCheckerHex = "#00C9F2";
        private const string BlackCheckerHex = "#E795EC";
        private const string NoneCheckerHex = "#4A5366";

        public static readonly DependencyProperty CheckerColorProperty =
            DependencyProperty.Register(nameof(CheckerColor), typeof(PlayerColor), 
                typeof(CheckerControl), new PropertyMetadata(PlayerColor.None, OnCheckerColorChanged));
        public CheckerControl()
        {
            InitializeComponent();
        }

        public PlayerColor CheckerColor
        {
            get { return (PlayerColor)GetValue(CheckerColorProperty); }
            set { SetValue(CheckerColorProperty, value); }
        }

        private static void OnCheckerColorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            CheckerControl changedChecker = (CheckerControl)sender;

            changedChecker.ApplyCheckerColor((PlayerColor)e.NewValue);
        }

        private void ApplyCheckerColor(PlayerColor checkerColor)
        {
            string colorHex = GetColorHex(checkerColor);

            CheckerEllipse.Fill = (Brush)new BrushConverter().ConvertFromString(colorHex);
        }

        private static string GetColorHex(PlayerColor checkerColor)
        {
            switch (checkerColor)
            {
                case PlayerColor.White:
                    return WhiteCheckerHex;
                case PlayerColor.Black:
                    return BlackCheckerHex;
                default:
                    return NoneCheckerHex;
            }
        }
    }
}
