using System;

using Backgammon.Server.Services.Data;

namespace Backgammon.Server.Domain
{
    internal static class MatchStateFactory
    {
        private const long InitialVersion = 1;

        public static MatchState CreateInitial(MatchSession matchSession)
        {
            if (matchSession == null)
            {
                throw new ArgumentNullException(nameof(matchSession));
            }

            var matchState = new MatchState
            {
                MatchId = matchSession.MatchId,
                RoomId = matchSession.RoomId,
                Phase = MatchPhase.WaitingForPlayers,
                Board = MatchBoardFactory.CreateInitial(),
                Version = InitialVersion
            };

            return matchState;
        }
    }
}