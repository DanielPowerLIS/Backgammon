using System;

namespace Backgammon.Client.Views.Game.Controls
{
    public class BoardPointSelection : EventArgs
    {
        private const int MinimumPointNumber = 1;
        private const int MaximumPointNumber = 24;
        private const int PinkBarPointNumber = 0;
        private const int BlueBarPointNumber = 25;

        public int PointNumber { get; }

        public BoardPointSelection(int pointNumber)
        {
            bool isBoardPoint = pointNumber >= MinimumPointNumber && pointNumber <= MaximumPointNumber;

            bool isBar = pointNumber == PinkBarPointNumber || pointNumber == BlueBarPointNumber;

            if (!isBoardPoint && !isBar)
            {
                throw new ArgumentOutOfRangeException(nameof(pointNumber));
            }

            PointNumber = pointNumber;
        }
    }
}