using System;

namespace Backgammon.Client.Views.Game.Controls
{
    public class BoardPointSelection : EventArgs
    {
        private const int MinimumPointNumber = 1;
        private const int MaximumPointNumber = 24;
        private const int PinkBarPointNumber = 0;
        private const int BlueBarPointNumber = 25;
        public const int BorneOffDestinationPointNumber = 26;

        public int PointNumber { get; }

        public BoardPointSelection(int pointNumber)
        {
            bool isBoardPoint = pointNumber >= MinimumPointNumber && pointNumber <= MaximumPointNumber;

            bool isBar = pointNumber == PinkBarPointNumber || pointNumber == BlueBarPointNumber;

            bool isBorneOff = pointNumber == BorneOffDestinationPointNumber;

            if (!isBoardPoint && !isBar && !isBorneOff)
            {
                throw new ArgumentOutOfRangeException(nameof(pointNumber));
            }

            PointNumber = pointNumber;
        }
    }
}