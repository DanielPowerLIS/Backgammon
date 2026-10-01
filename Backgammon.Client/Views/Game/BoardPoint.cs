namespace Backgammon.Client.Views.Game
{
    public class BoardPoint
    {
        public BoardPoint(int pointNumber)
        {
            Number = pointNumber;
            Owner = PlayerColor.None;
        }

        public int Number { get; set; }

        public PlayerColor Owner { get; set; }

        public int Count { get; set; }
    }
}
