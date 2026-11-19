using System;

namespace Backgammon.Client.Views.Game.Controls
{
    public class BoardPointSelection : EventArgs
    {
        private static readonly int MinimumPointNumber = 1;
        private static readonly int MaximumPointNumber = 24;

        public BoardPointSelection(int pointNumber)
        {
            if (pointNumber < MinimumPointNumber ||
                pointNumber > MaximumPointNumber)
            {
                throw new ArgumentOutOfRangeException(nameof(pointNumber));
            }

            PointNumber = pointNumber;
        }

        public int PointNumber { get; }
    }
}