namespace Backgammon.Client.Views.Game
{
    public class DiceResult
    {
        public DiceResult(int firstDieValue, int secondDieValue)
        {
            FirstDieValue = firstDieValue;
            SecondDieValue = secondDieValue;
        }

        public int FirstDieValue { get; }
        public int SecondDieValue { get; }

        public bool IsDouble
        {
            get {  return FirstDieValue == SecondDieValue; }
        }
    }
}
