using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Backgammon.Client.GameReference;

namespace Backgammon.Client.Views.Game.Controls
{
    public partial class BorneOffAreaControl : UserControl
    {
        private const int NoCheckers = 0;
        private const int TotalCheckers = 15;
        private const double RetiredCheckerSize = 28;
        private const double CheckerMargin = 2;

        private const string BlueAccentHex = "#00C9F2";
        private const string PinkAccentHex = "#E795EC";

        private readonly Brush _blueBrush;
        private readonly Brush _pinkBrush;

        private Brush _normalBorderBrush;
        private bool _isHighlighted;

        public CheckerColor AreaColor { get; private set; }

        public BorneOffAreaControl()
        {
            InitializeComponent();

            _blueBrush = (Brush)new BrushConverter().ConvertFromString(BlueAccentHex);

            _pinkBrush = (Brush)new BrushConverter().ConvertFromString(PinkAccentHex);

            _normalBorderBrush = _blueBrush;
            _isHighlighted = false;
            AreaColor = CheckerColor.None;

            MouseLeftButtonUp += OnAreaMouseLeftButtonUp;

            SetHighlighted(false);
        }

        public event EventHandler Selected;

        public void ApplyCheckers(int checkerCount, CheckerColor playerColor)
        {
            if (checkerCount < NoCheckers || checkerCount > TotalCheckers)
            {
                throw new ArgumentOutOfRangeException(nameof(checkerCount));
            }

            if (playerColor != CheckerColor.Blue && playerColor != CheckerColor.Pink)
            {
                throw new ArgumentOutOfRangeException(nameof(playerColor));
            }

            AreaColor = playerColor;

            string countFormat = Properties.Resources.TextBlock_BlueBorneOffCount;
            _normalBorderBrush = _blueBrush;

            if (playerColor == CheckerColor.Pink)
            {
                countFormat = Properties.Resources.TextBlock_PinkBorneOffCount;
                _normalBorderBrush = _pinkBrush;
            }

            textBlockCheckerCount.Text = string.Format(CultureInfo.CurrentCulture, countFormat, checkerCount);

            textBlockCheckerCount.Foreground = _normalBorderBrush;

            PopulateCheckers(checkerCount);

            SetHighlighted(false);
        }

        public void SetHighlighted(bool isHighlighted)
        {
            _isHighlighted = isHighlighted;
            IsHitTestVisible = isHighlighted;

            borderArea.BorderBrush = _normalBorderBrush;
            Cursor = Cursors.Arrow;

            if (isHighlighted)
            {
                borderArea.BorderBrush = Brushes.White;
                Cursor = Cursors.Hand;
            }
        }

        private void PopulateCheckers(int checkerCount)
        {
            stackPanelCheckers.Children.Clear();

            for (int checkerIndex = NoCheckers; checkerIndex < checkerCount; checkerIndex++)
            {
                var checker = new CheckerControl
                {
                    CheckerColor = AreaColor,
                    Width = RetiredCheckerSize,
                    Height = RetiredCheckerSize,
                    Margin = new Thickness(CheckerMargin),
                    IsHitTestVisible = false
                };

                stackPanelCheckers.Children.Add(checker);
            }
        }

        private void OnAreaMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseEventArgs)
        {
            if (!_isHighlighted)
            {
                return;
            }

            mouseEventArgs.Handled = true;

            EventHandler selectionHandler = Selected;

            if (selectionHandler != null)
            {
                selectionHandler.Invoke(this, EventArgs.Empty);
            }
        }
    }
}