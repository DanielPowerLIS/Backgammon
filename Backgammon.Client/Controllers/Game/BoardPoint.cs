using Backgammon.Client.GameReference;

namespace Backgammon.Client.Controllers.Game
{
    public class BoardPoint
    {
        public BoardPoint(int pointNumber)
        {
            Number = pointNumber;
            Owner = CheckerColor.None;
        }

        public int Number { get; set; }

        public CheckerColor Owner { get; set; }

        public int Count { get; set; }
    }
}
