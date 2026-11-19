using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using Backgammon.Client.GameReference;
using Backgammon.Client.Views.Game.Controls;

namespace Backgammon.Client.Views.Dialog
{
    public partial class DiceRollDialog : Window
    {
        private const int MinDieValue = 1;
        private const int MaxDieValue = 6;
        private readonly int AnimationIntervalMilliseconds = 100;
        private readonly int AnimationDurationMilliseconds = 1000;
        private const int UnrolledDieValue = 0;
        private readonly string PendingDieText = "—";

        private readonly bool _isHost;
        private readonly DispatcherTimer _animationTimer;

        private long _renderedVersion;
        private int _roundNumber;
        private int _hostDieValue;
        private int _guestDieValue;
        private int _elapsedMilliseconds;

        private bool _isOpeningRoll;
        private bool _isRolling;
        private bool _isTie;
        private bool _isComplete;
        private bool _animateHost;
        private bool _animateGuest;
        private bool _allowClose;

        public DiceRollDialog(bool isHost)
        {
            InitializeComponent();

            _isHost = isHost;

            _animationTimer = new DispatcherTimer();
            _animationTimer.Interval = TimeSpan.FromMilliseconds(AnimationIntervalMilliseconds);

            _animationTimer.Tick += OnAnimationTimerTick;
            buttonRollDie.Click += OnRollDieButtonClick;

            Closing += OnDialogClosing;
            Closed += OnDialogClosed;
        }

        public event Action<int> RollSelected;

        public void ApplyMatchPreparation(MatchPreparation preparation)
        {
            if (preparation == null || preparation.OpeningRoll == null)
            {
                throw new ArgumentException("The opening roll preparation is required.", nameof(preparation));
            }

            if (preparation.Version <= _renderedVersion)
            {
                return;
            }

            _renderedVersion = preparation.Version;
            _isOpeningRoll = preparation.Phase == MatchPhase.OpeningRoll;

            UpdateDiceValues(preparation.OpeningRoll);

            bool bothPlayersRolled = _hostDieValue > UnrolledDieValue && _guestDieValue > UnrolledDieValue;

            _isTie = _isOpeningRoll && bothPlayersRolled && _hostDieValue == _guestDieValue;

            _isComplete = preparation.Phase == MatchPhase.WaitingForRoll && bothPlayersRolled &&
                _hostDieValue != _guestDieValue;

            UpdateDice();
            UpdateControls();
        }

        public void CompleteRoll()
        {
            _isRolling = false;
            UpdateControls();
        }

        public void ShowRollError()
        {
            _isRolling = false;
            UpdateControls();

            textBlockRollStatus.Text = Properties.Resources.Message_OpeningRollError;
        }

        public void CloseForNavigation()
        {
            _allowClose = true;
            Close();
        }

        private void UpdateDiceValues(MatchOpeningRollState openingRoll)
        {
            bool newRound = openingRoll.RoundNumber != _roundNumber;
            bool hostChanged = newRound || openingRoll.HostDieValue != _hostDieValue;
            bool guestChanged = newRound || openingRoll.GuestDieValue != _guestDieValue;

            if (hostChanged)
            {
                _animateHost = openingRoll.HostDieValue > 0;
            }

            if (guestChanged)
            {
                _animateGuest = openingRoll.GuestDieValue > 0;
            }

            _roundNumber = openingRoll.RoundNumber;
            _hostDieValue = openingRoll.HostDieValue;
            _guestDieValue = openingRoll.GuestDieValue;

            if (hostChanged || guestChanged)
            {
                _elapsedMilliseconds = 0;
                _animationTimer.Stop();

                if (_animateHost || _animateGuest)
                {
                    _animationTimer.Start();
                }
            }
        }

        private void UpdateDice()
        {
            int hostDisplayValue = _hostDieValue;
            int guestDisplayValue = _guestDieValue;

            int faceCount = MaxDieValue - MinDieValue + 1;

            int animationStep = (_elapsedMilliseconds / AnimationIntervalMilliseconds) % faceCount;

            if (_animateHost)
            {
                hostDisplayValue = MinDieValue + animationStep;
            }

            if (_animateGuest)
            {
                guestDisplayValue = MaxDieValue - animationStep;
            }

            RenderDie(dieControlFirst, textBlockFirstDieValue, hostDisplayValue);

            RenderDie(dieControlSecond, textBlockSecondDieValue, guestDisplayValue);
        }

        private void RenderDie(DieControl dieControl, TextBlock valueText, int dieValue)
        {
            dieControl.Visibility = Visibility.Hidden;
            valueText.Text = PendingDieText;

            if (dieValue == 0)
            {
                return;
            }

            dieControl.Visibility = Visibility.Visible;
            dieControl.DieValue = dieValue;
            valueText.Text = dieValue.ToString();
        }

        private void UpdateControls()
        {
            int playerDieValue = GetPlayerDieValue();

            buttonRollDie.IsEnabled = _isOpeningRoll && !_isRolling && !_animationTimer.IsEnabled &&
                (playerDieValue == 0 || _isTie);

            buttonContinue.IsEnabled = _isComplete && !_isRolling && !_animationTimer.IsEnabled;

            UpdateRollStatus();
        }

        private int GetPlayerDieValue()
        {
            int playerDieValue = _guestDieValue;

            if (_isHost)
            {
                playerDieValue = _hostDieValue;
            }

            return playerDieValue;
        }

        private void UpdateRollStatus()
        {
            string status = Properties.Resources.Message_OpeningRollReady;
            int playerDieValue = GetPlayerDieValue();

            if (playerDieValue > 0)
            {
                status = Properties.Resources.Message_OpeningRollWaiting;
            }

            if (_isTie)
            {
                status = Properties.Resources.Message_OpeningRollTie;
            }

            if (_isComplete)
            {
                status = Properties.Resources.Message_OpeningRollOpponentStarts;
                bool hostStarts = _hostDieValue > _guestDieValue;

                if (_isHost == hostStarts)
                {
                    status = Properties.Resources.Message_OpeningRollYouStart;
                }
            }

            if (_isRolling || _animationTimer.IsEnabled)
            {
                status = Properties.Resources.Message_OpeningRollRolling;
            }

            textBlockRollStatus.Text = status;
        }

        private void OnRollDieButtonClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (!buttonRollDie.IsEnabled)
            {
                return;
            }

            Action<int> rollSelectedHandler = RollSelected;

            if (rollSelectedHandler == null)
            {
                return;
            }

            int roundNumber = _roundNumber;

            if (_isTie)
            {
                roundNumber++;
            }

            _isRolling = true;
            UpdateControls();

            rollSelectedHandler.Invoke(roundNumber);
        }

        private void OnAnimationTimerTick(object sender, EventArgs eventArgs)
        {
            _elapsedMilliseconds += AnimationIntervalMilliseconds;

            if (_elapsedMilliseconds >= AnimationDurationMilliseconds)
            {
                StopAnimation();
                return;
            }

            UpdateDice();
        }

        private void StopAnimation()
        {
            _animationTimer.Stop();

            _animateHost = false;
            _animateGuest = false;

            UpdateDice();
            UpdateControls();
        }

        private void OnContinueButtonClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (!buttonContinue.IsEnabled)
            {
                return;
            }

            _allowClose = true;
            Close();
        }

        private void OnDialogClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            if (!_allowClose)
            {
                cancelEventArgs.Cancel = true;
            }
        }

        private void OnDialogClosed(object sender, EventArgs eventArgs)
        {
            _animationTimer.Stop();
        }
    }
}