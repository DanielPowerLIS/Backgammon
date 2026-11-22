using System;

using Backgammon.Server.Services.Data;

namespace Backgammon.Server.Domain
{
    internal static class MatchCompletion
    {
        public static bool Complete(ActiveMatch activeMatch, int winnerPlayerId, MatchEndReason reason)
        {
            if (activeMatch == null)
            {
                throw new ArgumentNullException(nameof(activeMatch));
            }

            bool completed = false;
            MatchState matchState = activeMatch.State;

            if (matchState.Phase == MatchPhase.Finished)
            {
                return completed;
            }

            if (matchState.Phase != MatchPhase.WaitingForRoll &&
                matchState.Phase != MatchPhase.MovingCheckers)
            {
                throw new ArgumentException("The match has no active turn to complete.");
            }

            MatchResult result = CreateResult(activeMatch.Session, winnerPlayerId, reason);

            activeMatch.Clock.Stop();

            matchState.Result = result;
            matchState.Phase = MatchPhase.Finished;
            matchState.Turn.CanUndo = false;
            matchState.Turn.CanConfirm = false;
            matchState.Turn.AvailableMoves = Array.Empty<CheckerMove>();

            activeMatch.TurnHistory.Clear();

            completed = true;

            return completed;
        }

        private static MatchResult CreateResult(MatchSession matchSession, int winnerPlayerId, MatchEndReason reason)
        {
            if (reason != MatchEndReason.TimeExpired && reason != MatchEndReason.AllCheckersBorneOff)
            {
                throw new ArgumentOutOfRangeException(nameof(reason));
            }

            int loserPlayerId;

            if (winnerPlayerId == matchSession.PlayerHostId)
            {
                loserPlayerId = matchSession.PlayerGuestId;
            }
            else if (winnerPlayerId == matchSession.PlayerGuestId)
            {
                loserPlayerId = matchSession.PlayerHostId;
            }
            else
            {
                throw new ArgumentException("The winner does not belong to the match.", nameof(winnerPlayerId));
            }

            var result = new MatchResult
            {
                WinnerPlayerId = winnerPlayerId,
                LoserPlayerId = loserPlayerId,
                Reason = reason
            };

            return result;
        }
    }
}