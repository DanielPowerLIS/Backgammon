using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Backgammon.Client.Views.Game.Controls
{
    public partial class MatchStartControl : UserControl
    {
        private static readonly double RefreshIntervalMilliseconds = 100;

        private readonly DispatcherTimer _countdownTimer;
        private readonly Stopwatch _countdownClock;

        private TimeSpan _countdownDuration;

        public MatchStartControl()
        {
            InitializeComponent();

            _countdownClock = new Stopwatch();
            _countdownDuration = TimeSpan.Zero;

            _countdownTimer = new DispatcherTimer();
            _countdownTimer.Interval = TimeSpan.FromMilliseconds(RefreshIntervalMilliseconds);

            _countdownTimer.Tick += OnCountdownTick;
            Unloaded += OnControlUnloaded;
        }

        public void ShowWaiting(string message)
        {
            StopCountdown();

            textBlockAnnouncement.Text = message;
            textBlockCountdown.Text = string.Empty;
            textBlockCountdown.Visibility = Visibility.Collapsed;
        }

        public void StartCountdown(string message, TimeSpan duration)
        {
            StopCountdown();

            textBlockAnnouncement.Text = message;
            textBlockCountdown.Visibility = Visibility.Visible;

            _countdownDuration = duration;
            _countdownClock.Restart();

            UpdateCountdownDisplay();

            if (_countdownDuration > TimeSpan.Zero)
            {
                _countdownTimer.Start();
            }
        }

        public void StopCountdown()
        {
            _countdownTimer.Stop();
            _countdownClock.Reset();

            _countdownDuration = TimeSpan.Zero;
        }

        private void OnCountdownTick(object sender, EventArgs eventArgs)
        {
            UpdateCountdownDisplay();
        }

        private void UpdateCountdownDisplay()
        {
            TimeSpan remainingTime = _countdownDuration - _countdownClock.Elapsed;

            int remainingSeconds = Math.Max(0, (int)Math.Ceiling(remainingTime.TotalSeconds));

            textBlockCountdown.Text = remainingSeconds.ToString();

            if (remainingSeconds == 0)
            {
                StopCountdown();
            }
        }

        private void OnControlUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            StopCountdown();
        }
    }
}