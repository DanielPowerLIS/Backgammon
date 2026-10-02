using Backgammon.Client.Models.Game;
using Backgammon.Client.Views.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Backgammon.Client.Views.Dialog
{
    public partial class DiceRollDialog : Window
    {
        private const int MinDieValue = 1;
        private const int MaxDieValue = 6;
        private const int AnimationIntervalMilliseconds = 100;
        private const int AnimationDurationMilliseconds = 1000;
        private readonly Random _random = new Random();
        private readonly DispatcherTimer _animationTimer = new DispatcherTimer();
        private readonly PlayerColor _currentPlayerColor;
        private int _elapsedMilliseconds;
        public DiceRollDialog(PlayerColor currentPlayerColor)
        {
            InitializeComponent();
            _currentPlayerColor = currentPlayerColor;
            SetupDice();
            SetupTimer();
        }

        public DiceResult Result { get; private set; }

        private void SetupDice()
        {
            FirstDie.DieValue = MinDieValue;
            SecondDie.DieValue = MinDieValue;
        }

        private void SetupTimer()
        {
            _animationTimer.Interval = TimeSpan.FromMilliseconds(AnimationIntervalMilliseconds);
            _animationTimer.Tick += OnAnimationTimerTick;
            _animationTimer.Start();
        }

        private void OnAnimationTimerTick(object sender, EventArgs e)
        {
            _elapsedMilliseconds += AnimationIntervalMilliseconds;

            FirstDie.DieValue = _random.Next(MinDieValue, MaxDieValue + 1);
            SecondDie.DieValue = _random.Next(MinDieValue, MaxDieValue + 1);

            FirstDieValueText.Text = FirstDie.DieValue.ToString();
            SecondDieValueText.Text = SecondDie.DieValue.ToString();

            if (_elapsedMilliseconds >= AnimationDurationMilliseconds)
            {
                StopAnimation();
            }
        }

        private void StopAnimation()
        {
            _animationTimer.Stop();

            Result = new DiceResult(FirstDie.DieValue, SecondDie.DieValue);

            RollStatusText.Text = "¡Resultado!";
            ContinueButton.IsEnabled = true;
        }

        private void OnContinueButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
