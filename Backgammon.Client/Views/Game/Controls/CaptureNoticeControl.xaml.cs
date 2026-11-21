using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Backgammon.Client.Views.Game.Controls
{
    public partial class CaptureNoticeControl : UserControl
    {
        private static readonly TimeSpan DisplayDuration = TimeSpan.FromSeconds(2);

        private readonly DispatcherTimer _displayTimer;

        public CaptureNoticeControl()
        {
            InitializeComponent();

            _displayTimer = new DispatcherTimer();
            _displayTimer.Interval = DisplayDuration;
            _displayTimer.Tick += OnDisplayTimerTick;

            Unloaded += OnControlUnloaded;
        }

        public event EventHandler DisplayFinished;

        public void ShowMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("The capture message is required.", nameof(message));
            }

            _displayTimer.Stop();

            textBlockCaptureMessage.Text = message;

            _displayTimer.Start();
        }

        public void StopNotice()
        {
            _displayTimer.Stop();
        }

        private void OnDisplayTimerTick(object sender, EventArgs eventArgs)
        {
            StopNotice();

            EventHandler displayFinishedHandler = DisplayFinished;

            if (displayFinishedHandler != null)
            {
                displayFinishedHandler.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnControlUnloaded(
            object sender,
            RoutedEventArgs routedEventArgs)
        {
            StopNotice();
        }
    }
}