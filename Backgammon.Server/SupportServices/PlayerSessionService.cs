using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using System.Threading;

using Backgammon.Server.Services.Data;

namespace Backgammon.Server.SupportServices
{
    internal class PlayerSessionService
    {
        private static readonly TimeSpan SessionInactivityTimeout =
            TimeSpan.FromMinutes(30);

        private static readonly Dictionary<string, PlayerSession> ActiveSessions =
            new Dictionary<string, PlayerSession>();

        private static readonly object SessionSynchronization = new object();

        private static readonly int SessionTokenLength = 32;

        private static int _lastGuestPlayerId = 0;

        public PlayerSession CreateSession(int playerId, string playerUsername)
        {
            PlayerSession playerSession = CreatePlayerSession(
                playerId, playerUsername, false);

            return playerSession;
        }

        public PlayerSession CreateGuestSession()
        {
            var usernameGenerator = new GuestUsernameGenerator();

            string guestUsername = usernameGenerator.Generate();

            int guestPlayerId = Interlocked.Decrement(ref _lastGuestPlayerId);

            PlayerSession guestSession = CreatePlayerSession(
                guestPlayerId, guestUsername, true);

            return guestSession;
        }

        public void RemoveSession(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return;
            }

            lock (SessionSynchronization)
            {
                ActiveSessions.Remove(sessionId);
            }
        }

        public PlayerSession GetSession(string sessionId, bool registerActivity)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new UnauthorizedAccessException();
            }

            lock (SessionSynchronization)
            {
                DateTime currentTimeUtc = DateTime.UtcNow;
                PlayerSession storedSession = GetStoredSession(sessionId, currentTimeUtc);
                    
                if (registerActivity)
                {
                    storedSession.ExpiresAtUtc =
                        currentTimeUtc.Add(SessionInactivityTimeout);
                }

                PlayerSession sessionCopy = CopySession(storedSession);

                return sessionCopy;
            }
        }

        public int GetProfileId(string sessionId)
        {
            PlayerSession activeSession = GetSession(sessionId, true);

            int profileId = activeSession.PlayerId;

            return profileId;
        }

        private PlayerSession CreatePlayerSession(
            int playerId, string playerUsername, bool isGuest)
        {
            RemoveExpiredSessions();

            var playerSession = new PlayerSession
            {
                SessionId = CreateSessionId(),
                PlayerId = playerId,
                Username = playerUsername,
                IsGuest = isGuest,
                ExpiresAtUtc = DateTime.UtcNow.Add(SessionInactivityTimeout)
            };

            lock (SessionSynchronization)
            {
                ActiveSessions.Add(playerSession.SessionId, CopySession(playerSession));
            }

            return playerSession;
        }

        private void RemoveExpiredSessions()
        {
            lock (SessionSynchronization)
            {
                DateTime currentTimeUtc = DateTime.UtcNow;

                string[] expiredSessionIds = ActiveSessions
                    .Where(sessionEntry =>
                        sessionEntry.Value.ExpiresAtUtc <= currentTimeUtc)
                    .Select(sessionEntry => sessionEntry.Key)
                    .ToArray();

                foreach (string expiredSessionId in expiredSessionIds)
                {
                    ActiveSessions.Remove(expiredSessionId);
                }
            }
        }

        private PlayerSession GetStoredSession(string sessionId, DateTime currentTimeUtc)
        {
            if (!ActiveSessions.TryGetValue(sessionId, out PlayerSession storedSession))
            {
                throw new UnauthorizedAccessException();
            }

            if (storedSession.ExpiresAtUtc <= currentTimeUtc)
            {
                ActiveSessions.Remove(sessionId);

                throw new UnauthorizedAccessException();
            }

            return storedSession;
        }

        private PlayerSession CopySession(PlayerSession sourceSession)
        {
            var sessionCopy = new PlayerSession
            {
                SessionId = sourceSession.SessionId,
                PlayerId = sourceSession.PlayerId,
                Username = sourceSession.Username,
                IsGuest = sourceSession.IsGuest,
                ExpiresAtUtc = sourceSession.ExpiresAtUtc
            };

            return sessionCopy;
        }

        private string CreateSessionId()
        {
            byte[] sessionToken = new byte[SessionTokenLength];

            using (RandomNumberGenerator tokenGenerator =
                RandomNumberGenerator.Create())
            {
                tokenGenerator.GetBytes(sessionToken);
            }

            string sessionId = Convert.ToBase64String(sessionToken);

            return sessionId;
        }
    }
}
