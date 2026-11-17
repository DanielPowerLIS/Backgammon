using System;
using System.ServiceModel;

using Backgammon.Client.GameReference;

namespace Backgammon.Client.Controllers.Callbacks
{
    [CallbackBehavior(UseSynchronizationContext = false)]
    internal class GameCallback : IGameServiceCallback
    {
        public event Action<MatchState> MatchStateUpdated;

        public void OnMatchStateUpdated(MatchState matchState)
        {
            Action<MatchState> stateUpdatedHandler = MatchStateUpdated;

            if (stateUpdatedHandler != null)
            {
                stateUpdatedHandler.Invoke(matchState);
            }
        }
    }
}