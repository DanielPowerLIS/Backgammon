using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Threading;

using Backgammon.Server.Services;
using Backgammon.Server.Services.Data;
using Backgammon.Server.SupportServices.Logging;

namespace Backgammon.Server.SupportServices
{
    internal class PlayerSessionManager
    {
        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(PlayerSessionManager));

        private static readonly string SessionExpiredErrorCode = "SessionExpired";

        private static readonly TimeSpan SessionInactivityTimeout =
            TimeSpan.FromMinutes(30);

        private static readonly ConcurrentDictionary<string, PlayerSession> ActiveSessions =
            new ConcurrentDictionary<string, PlayerSession>();

        private static readonly int SessionTokenLength = 32;

        private static int _lastGuestPlayerId = 0;

        public PlayerSession CreateSession(int playerId, string playerUsername)
        {
            PlayerSession playerSession = CreatePlayerSession(playerId, playerUsername, false);

            return playerSession;
        }

        public PlayerSession CreateGuestSession()
        {
            var usernameGenerator = new GuestUsernameGenerator();

            string guestUsername = usernameGenerator.Generate();

            int guestPlayerId = Interlocked.Decrement(ref _lastGuestPlayerId);

            PlayerSession guestSession = CreatePlayerSession(guestPlayerId, guestUsername, true);

            return guestSession;
        }

        public void RemoveSession(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return;
            }

            PlayerSession storedSession;

            try
            {
                storedSession = FindStoredSession(sessionId);
            }
            catch (UnauthorizedAccessException sessionException)
            {
                ApplicationLogger.LogWarning(
                    "Session removal was skipped because the session no longer exists. Reason: {0}",
                    sessionException.Message);

                return;
            }

            lock (storedSession.Queue)
            {
                RemoveStoredSession(sessionId);
            }
        }

        public PlayerSession GetSession(string sessionId, bool registerActivity)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new UnauthorizedAccessException();
            }

            PlayerSession storedSession = FindStoredSession(sessionId);

            lock (storedSession.Queue)
            {
                FindStoredSession(sessionId);

                DateTime currentTimeUtc = DateTime.UtcNow;

                ValidateExpiration(storedSession, currentTimeUtc);

                if (registerActivity)
                {
                    storedSession.ExpiresAtUtc = currentTimeUtc.Add(SessionInactivityTimeout);
                }

                PlayerSession sessionCopy = CopySession(storedSession);

                return sessionCopy;
            }
        }

        public int GetProfileId(string sessionId)
        {
            try
            {
                PlayerSession activeSession = GetSession(sessionId, true);

                int profileId = activeSession.PlayerId;

                return profileId;
            }
            catch (UnauthorizedAccessException sessionException)
            {
                ApplicationLogger.LogWarning(
                    "The profile operation was rejected because the session is invalid or expired. Reason: {0}",
                    sessionException.Message);

                throw CreatePlayerSessionFault(SessionExpiredErrorCode);
            }
        }

        private PlayerSession FindStoredSession(string sessionId)
        {
            bool sessionFound = ActiveSessions.TryGetValue(sessionId, out PlayerSession storedSession);

            if (!sessionFound || storedSession == null)
            {
                throw new UnauthorizedAccessException();
            }

            return storedSession;
        }

        private void StoreSession(PlayerSession playerSession)
        {
            PlayerSession storedSession = CopySession(playerSession);

            bool sessionStored = ActiveSessions.TryAdd(storedSession.SessionId, storedSession);

            while (!sessionStored)
            {
                storedSession.SessionId = CreateSessionId();

                sessionStored = ActiveSessions.TryAdd(storedSession.SessionId, storedSession);
            }

            playerSession.SessionId = storedSession.SessionId;
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
                ExpiresAtUtc = DateTime.UtcNow.Add(SessionInactivityTimeout),
                IsPlaying = false
            };

            StoreSession(playerSession);

            return playerSession;
        }

        private bool RemoveStoredSession(string sessionId)
        {
            bool sessionRemoved = ActiveSessions.TryRemove(sessionId, out _);

            return sessionRemoved;
        }

        private void RemoveExpiredSessions()
        {
            foreach (PlayerSession storedSession in ActiveSessions.Values)
            {
                lock (storedSession.Queue)
                {
                    DateTime currentTimeUtc = DateTime.UtcNow;

                    if (storedSession.ExpiresAtUtc <= currentTimeUtc)
                    {
                        RemoveStoredSession(storedSession.SessionId);
                    }
                }
            }
        }

        private void ValidateExpiration(PlayerSession storedSession, DateTime currentTimeUtc)
        {
            if (storedSession.ExpiresAtUtc <= currentTimeUtc)
            {
                RemoveStoredSession(storedSession.SessionId);

                throw new UnauthorizedAccessException();
            }
        }

        private PlayerSession CopySession(PlayerSession sourceSession)
        {
            var sessionCopy = new PlayerSession
            {
                SessionId = sourceSession.SessionId,
                PlayerId = sourceSession.PlayerId,
                Username = sourceSession.Username,
                IsGuest = sourceSession.IsGuest,
                ExpiresAtUtc = sourceSession.ExpiresAtUtc,
                IsPlaying = sourceSession.IsPlaying
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

        private FaultException<PlayerProfileFault> CreatePlayerSessionFault(
            string errorCode)
        {
            var profileFault = new PlayerProfileFault
            {
                ErrorCode = errorCode
            };

            var faultException =
                new FaultException<PlayerProfileFault>(profileFault);

            return faultException;
        }
    }
}
