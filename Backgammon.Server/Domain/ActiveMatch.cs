using System;

using Backgammon.Server.Services.Data;
using Backgammon.Server.SupportServices;

namespace Backgammon.Server.Domain
{
    internal class ActiveMatch
    {
        public ActiveMatch(MatchSession matchSession)
        {
            if (matchSession == null)
            {
                throw new ArgumentNullException(nameof(matchSession));
            }

            Session = matchSession;
            State = MatchStateFactory.CreateInitial(matchSession);
            Queue = new Entrance();
        }

        public MatchSession Session { get; }

        public MatchState State { get; }

        public Entrance Queue { get; }

        public bool HostReady { get; set; }

        public bool GuestReady { get; set; }
    }
}