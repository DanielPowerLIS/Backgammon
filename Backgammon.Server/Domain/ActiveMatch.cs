using System;
using System.Threading;

using Backgammon.Server.Services.Data;
using Backgammon.Server.SupportServices;
using Backgammon.Server.Services.Contracts;

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

        public IGameCallback HostCallback { get; set; }

        public IGameCallback GuestCallback { get; set; }

        public Timer CountdownTimer { get; set; }

        public void StopCountdown()
        {
            Timer countdownTimer;

            lock (Queue)
            {
                countdownTimer = CountdownTimer;
                CountdownTimer = null;
            }

            if (countdownTimer != null)
            {
                countdownTimer.Dispose();
            }
        }
    }
}