using System;

using Backgammon.Server.Services.Data;

namespace Backgammon.Server.Domain
{
    internal static class MatchPreparationFactory
    {
        public static MatchPreparation CreateInitial(MatchSession matchSession)
        {
            if (matchSession == null)
            {
                throw new ArgumentNullException(nameof(matchSession));
            }

            var preparation = new MatchPreparation
            {
                MatchId = matchSession.MatchId,
                RoomId = matchSession.RoomId
            };

            return preparation;
        }

        public static MatchPreparation Copy(MatchPreparation preparation)
        {
            if (preparation == null)
            {
                throw new ArgumentNullException(nameof(preparation));
            }

            var preparationCopy = new MatchPreparation
            {
                MatchId = preparation.MatchId,
                RoomId = preparation.RoomId,
                Phase = preparation.Phase,
                Version = preparation.Version,
                CountdownEndsAtUtc = preparation.CountdownEndsAtUtc,
                ServerTimeUtc = DateTime.UtcNow,
                OpeningRoll = CopyOpeningRoll(preparation.OpeningRoll)
            };

            return preparationCopy;
        }

        private static MatchOpeningRollState CopyOpeningRoll(MatchOpeningRollState openingRoll)
        {
            if (openingRoll == null)
            {
                throw new ArgumentNullException(nameof(openingRoll));
            }

            var openingRollCopy = new MatchOpeningRollState
            {
                RoundNumber = openingRoll.RoundNumber,
                HostDieValue = openingRoll.HostDieValue,
                GuestDieValue = openingRoll.GuestDieValue
            };

            return openingRollCopy;
        }
    }
}