using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using Backgammon.Client.GameReference;

namespace Backgammon.Client.Views.Game.Controls
{
    public partial class MatchClockControl : UserControl
    {
        private const int RefreshIntervalMilliseconds = 100;
        private const string ClockTimeFormat = @"mm\:ss";

        private readonly DispatcherTimer _displayTimer;
        private readonly Stopwatch _displayClock;

        private TimeSpan _globalTime;
        private TimeSpan _graceTime;
        private bool _isLocalTurn;
        private bool _isRunning;

        public MatchClockControl()
        {
            InitializeComponent();

            _displayClock = new Stopwatch();
            _globalTime = TimeSpan.Zero;
            _graceTime = TimeSpan.Zero;
            _isLocalTurn = false;
            _isRunning = false;

            _displayTimer = new DispatcherTimer();
            _displayTimer.Interval = TimeSpan.FromMilliseconds(
                RefreshIntervalMilliseconds);

            _displayTimer.Tick += OnDisplayTimerTick;
            Unloaded += OnControlUnloaded;

            UpdateClockDisplay();
        }

        public void ApplyClock(MatchClockState clockState, bool isHost, bool isLocalTurn)
        {
            if (clockState == null)
            {
                throw new ArgumentNullException(nameof(clockState));
            }

            StopDisplay();

            _globalTime = clockState.GuestRemainingTime;

            if (isHost)
            {
                _globalTime = clockState.HostRemainingTime;
            }

            _graceTime = clockState.GraceRemainingTime;
            _isLocalTurn = isLocalTurn;
            _isRunning = clockState.IsRunning;

            _displayClock.Restart();

            UpdateClockDisplay();

            if (_isRunning)
            {
                _displayTimer.Start();
            }
            else
            {
                _displayClock.Stop();
            }
        }

        public void StopDisplay()
        {
            _displayTimer.Stop();
            _displayClock.Stop();
        }

        private void UpdateClockDisplay()
        {
            TimeSpan elapsedTime = TimeSpan.Zero;

            if (_isRunning)
            {
                elapsedTime = _displayClock.Elapsed;
            }

            TimeSpan graceRemaining = GetNonNegativeTime(_graceTime - elapsedTime);

            TimeSpan globalRemaining = _globalTime;

            if (_isRunning && _isLocalTurn)
            {
                TimeSpan consumedTime = GetNonNegativeTime(elapsedTime - _graceTime);

                globalRemaining = GetNonNegativeTime(_globalTime - consumedTime);
            }

            textBlockGraceTime.Text = FormatTime(graceRemaining);
            textBlockGlobalTime.Text = FormatTime(globalRemaining);
        }

        private static string FormatTime(TimeSpan time)
        {
            TimeSpan remainingTime = GetNonNegativeTime(time);

            double seconds = Math.Ceiling(remainingTime.TotalSeconds);
            TimeSpan displayTime = TimeSpan.FromSeconds(seconds);

            string formattedTime = displayTime.ToString(ClockTimeFormat, CultureInfo.InvariantCulture);

            return formattedTime;
        }

        private static TimeSpan GetNonNegativeTime(TimeSpan time)
        {
            TimeSpan remainingTime = time;

            if (remainingTime < TimeSpan.Zero)
            {
                remainingTime = TimeSpan.Zero;
            }

            return remainingTime;
        }

        private void OnDisplayTimerTick(object sender, EventArgs eventArgs)
        {
            UpdateClockDisplay();
        }

        private void OnControlUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            StopDisplay();
        }
    }
}