using System;
using System.Diagnostics;

using Backgammon.Server.Services.Data;

namespace Backgammon.Server.Domain
{
    internal sealed class MatchClock
    {
        private const int TurnGraceSeconds = 10;

        private static readonly TimeSpan GraceDuration = TimeSpan.FromSeconds(TurnGraceSeconds);

        private readonly MatchClockState _clockState;
        private readonly Stopwatch _turnClock;

        private bool _isHostTurn;
        private TimeSpan _turnRemainingTime;

        public TimeSpan RemainingTurnTime
        {
            get
            {
                TimeSpan remainingTime = TimeSpan.Zero;

                if (_clockState.IsRunning)
                {
                    remainingTime = GetNonNegativeTime(GraceDuration + _turnRemainingTime - _turnClock.Elapsed);
                }

                return remainingTime;
            }
        }

        public bool HasExpired
        {
            get
            {
                bool hasExpired = _clockState.IsRunning && RemainingTurnTime == TimeSpan.Zero;

                return hasExpired;
            }
        }

        public MatchClock(MatchClockState clockState)
        {
            if (clockState == null)
            {
                throw new ArgumentNullException(nameof(clockState));
            }

            _clockState = clockState;
            _turnClock = new Stopwatch();
            _isHostTurn = false;
            _turnRemainingTime = TimeSpan.Zero;
        }

        public void StartTurn(bool isHostTurn)
        {
            if (_clockState.IsRunning)
            {
                throw new InvalidOperationException("The current turn clock must be stopped first.");
            }

            TimeSpan remainingTime = GetPlayerRemainingTime(isHostTurn);

            if (remainingTime <= TimeSpan.Zero)
            {
                throw new ArgumentException("The player has no remaining global time.");
            }

            _isHostTurn = isHostTurn;
            _turnRemainingTime = remainingTime;

            _turnClock.Restart();
            _clockState.IsRunning = true;

            UpdateState();
        }

        public void UpdateState()
        {
            if (_clockState.IsRunning)
            {
                TimeSpan elapsedTime = _turnClock.Elapsed;

                _clockState.GraceRemainingTime = GetNonNegativeTime(GraceDuration - elapsedTime);

                TimeSpan consumedTime = GetNonNegativeTime(elapsedTime - GraceDuration);

                TimeSpan remainingTime = GetNonNegativeTime(_turnRemainingTime - consumedTime);

                if (_isHostTurn)
                {
                    _clockState.HostRemainingTime = remainingTime;
                }
                else
                {
                    _clockState.GuestRemainingTime = remainingTime;
                }
            }

            _clockState.ServerTimeUtc = DateTime.UtcNow;
        }

        public bool Stop()
        {
            _turnClock.Stop();

            UpdateState();

            bool hasExpired = HasExpired;

            _clockState.IsRunning = false;
            _clockState.GraceRemainingTime = TimeSpan.Zero;

            return hasExpired;
        }

        private TimeSpan GetPlayerRemainingTime(bool isHostTurn)
        {
            TimeSpan remainingTime = _clockState.GuestRemainingTime;

            if (isHostTurn)
            {
                remainingTime = _clockState.HostRemainingTime;
            }

            return remainingTime;
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
    }
}