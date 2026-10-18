using System;

using Backgammon.Client.AuthenticationReference;

namespace Backgammon.Client.Controllers.Session
{
    internal static class PlayerSessionContext
    {
        private static PlayerSession _currentSession;

        public static void StoreSession(PlayerSession playerSession)
        {
            if (playerSession == null)
            {
                throw new ArgumentNullException(nameof(playerSession));
            }

            _currentSession = playerSession;
        }

        public static PlayerSession GetSession()
        {
            if (_currentSession == null)
            {
                throw new InvalidOperationException(
                    "No player session has been stored.");
            }

            return _currentSession;
        }

        public static void ClearSession()
        {
            _currentSession = null;
        }
    }
}