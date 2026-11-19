using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Threading;

using Backgammon.Client.GameReference;

namespace Backgammon.Client.Views.Game.Controls
{
    public partial class TurnDiceControl : UserControl
    {
        private static readonly int NoDieSelected = 0;
        private static readonly int FirstDieIndex = 1;
        private static readonly int SecondDieIndex = 2;

        private const int MinimumDieValue = 1;
        private const int MaximumDieValue = 6;

        private static readonly TimeSpan AnimationInterval = TimeSpan.FromMilliseconds(100);
        private static readonly TimeSpan AnimationDuration = TimeSpan.FromSeconds(1);

        private readonly DispatcherTimer _animationTimer;
        private readonly Stopwatch _animationClock;

        private bool _hasDice;
        private CheckerColor _playerColor;

        private MatchDiceState _diceState;
        private bool _canRoll;
        private bool _canSelect;
        private bool _isRolling;

        private int _previousDieIndex;

        public TurnDiceControl()
        {
            InitializeComponent();

            _diceState = new MatchDiceState();
            _canRoll = false;
            _canSelect = false;
            _isRolling = false;
            _hasDice = false;
            _playerColor = CheckerColor.None;

            _animationClock = new Stopwatch();
            _animationTimer = new DispatcherTimer();
            _animationTimer.Interval = AnimationInterval;

            _animationTimer.Tick += OnAnimationTimerTick;
            buttonRollDice.Click += OnRollDiceClick;
            Unloaded += OnControlUnloaded;

            _previousDieIndex = NoDieSelected;

            radioButtonFirstDie.Checked += OnDieSelectionChanged;
            radioButtonSecondDie.Checked += OnDieSelectionChanged;
        }

        public int SelectedDieIndex
        {
            get
            {
                int selectedDieIndex = NoDieSelected;

                if (radioButtonFirstDie.IsChecked == true)
                {
                    selectedDieIndex = FirstDieIndex;
                }
                else if (radioButtonSecondDie.IsChecked == true)
                {
                    selectedDieIndex = SecondDieIndex;
                }

                return selectedDieIndex;
            }
        }

        public event EventHandler RollSelected;

        public event EventHandler SelectedDieChanged;

        public void ApplyMatchState(MatchState matchState, bool isLocalTurn, CheckerColor playerColor)
        {
            if (matchState == null)
            {
                throw new ArgumentNullException(nameof(matchState));
            }

            if (matchState.Dice == null)
            {
                throw new ArgumentException("The match dice state is required.", nameof(matchState));
            }

            bool hadDice = _hasDice;

            _diceState = matchState.Dice;
            _playerColor = playerColor;

            _hasDice = matchState.Phase == MatchPhase.MovingCheckers &&
                _diceState.FirstDieValue > 0 && _diceState.SecondDieValue > 0;

            _canRoll = isLocalTurn && matchState.Phase == MatchPhase.WaitingForRoll;
            _canSelect = isLocalTurn && _hasDice;

            if (!_hasDice)
            {
                CancelAnimation();

                GridDice.Visibility = Visibility.Collapsed;
                textBlockDoubles.Visibility = Visibility.Collapsed;
            }
            else if (!hadDice)
            {
                ShowDice(_playerColor);
                StartAnimation();
            }
            else if (!_animationTimer.IsEnabled)
            {
                ShowDice(_playerColor);
            }

            UpdateAvailability();
            UpdateSelection();
        }

        public void SetRolling(bool isRolling)
        {
            _isRolling = isRolling;

            UpdateAvailability();
            UpdateSelection();
        }

        private void ShowDice(CheckerColor playerColor)
        {
            dieControlFirst.DieColor = playerColor;
            dieControlSecond.DieColor = playerColor;

            dieControlFirst.DieValue = _diceState.FirstDieValue;
            dieControlSecond.DieValue = _diceState.SecondDieValue;

            textBlockFirstDieUses.Text = string.Format(
                CultureInfo.CurrentCulture,
                Properties.Resources.Label_DieUsesFormat,
                _diceState.FirstDieRemainingUses);

            textBlockSecondDieUses.Text = string.Format(
                CultureInfo.CurrentCulture,
                Properties.Resources.Label_DieUsesFormat,
                _diceState.SecondDieRemainingUses);

            GridDice.Visibility = Visibility.Visible;

            if (_diceState.FirstDieValue == _diceState.SecondDieValue)
            {
                textBlockDoubles.Visibility = Visibility.Visible;
            }
        }

        private void UpdateAvailability()
        {
            bool isBusy = _isRolling || _animationTimer.IsEnabled;

            buttonRollDice.IsEnabled = _canRoll && !isBusy;

            radioButtonFirstDie.IsEnabled = _canSelect && !isBusy && _diceState.FirstDieRemainingUses > 0;

            radioButtonSecondDie.IsEnabled = _canSelect && !isBusy && _diceState.SecondDieRemainingUses > 0;
        }

        private void UpdateSelection()
        {
            bool firstSelected = radioButtonFirstDie.IsChecked == true && radioButtonFirstDie.IsEnabled;

            bool secondSelected = radioButtonSecondDie.IsChecked == true && radioButtonSecondDie.IsEnabled;

            if (!firstSelected && !secondSelected)
            {
                radioButtonFirstDie.IsChecked = radioButtonFirstDie.IsEnabled;

                radioButtonSecondDie.IsChecked = !radioButtonFirstDie.IsEnabled &&
                    radioButtonSecondDie.IsEnabled;
            }

            ApplyDieSelection();
        }

        private void OnRollDiceClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (!_canRoll || _isRolling)
            {
                return;
            }

            EventHandler rollHandler = RollSelected;

            if (rollHandler != null)
            {
                rollHandler.Invoke(this, EventArgs.Empty);
            }
        }

        private void StartAnimation()
        {
            CancelAnimation();

            textBlockFirstDieUses.Text = string.Empty;
            textBlockSecondDieUses.Text = string.Empty;
            textBlockDoubles.Visibility = Visibility.Collapsed;

            _animationClock.Restart();

            UpdateAnimatedFaces();

            _animationTimer.Start();
        }

        private void UpdateAnimatedFaces()
        {
            int faceCount = MaximumDieValue - MinimumDieValue + 1;

            int animationStep = (int)(_animationClock.Elapsed.TotalMilliseconds /
                AnimationInterval.TotalMilliseconds) % faceCount;

            dieControlFirst.DieValue = MinimumDieValue + animationStep;
            dieControlSecond.DieValue = MaximumDieValue - animationStep;
        }

        private void OnAnimationTimerTick(object sender, EventArgs eventArgs)
        {
            if (_animationClock.Elapsed >= AnimationDuration)
            {
                CompleteAnimation();
                return;
            }

            UpdateAnimatedFaces();
        }

        private void CompleteAnimation()
        {
            CancelAnimation();

            if (_hasDice)
            {
                ShowDice(_playerColor);
            }

            UpdateAvailability();
            UpdateSelection();
        }

        private void CancelAnimation()
        {
            _animationTimer.Stop();
            _animationClock.Reset();
        }

        private void OnControlUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            CancelAnimation();
        }

        private void OnDieSelectionChanged(object sender, RoutedEventArgs routedEventArgs)
        {
            ApplyDieSelection();
        }

        private void ApplyDieSelection()
        {
            int selectedDieIndex = SelectedDieIndex;

            if (selectedDieIndex == _previousDieIndex)
            {
                return;
            }

            _previousDieIndex = selectedDieIndex;

            EventHandler selectionHandler = SelectedDieChanged;

            if (selectionHandler != null)
            {
                selectionHandler.Invoke(this, EventArgs.Empty);
            }
        }
    }
}