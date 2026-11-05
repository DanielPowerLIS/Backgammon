using System;
using System.Security.Cryptography;
using System.ServiceModel;

using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Services.Data;
using Backgammon.Server.SupportServices;
using Backgammon.Server.SupportServices.Logging;

namespace Backgammon.Server.Services
{
    public class GuestAccessService : IGuestAccessService
    {
        private readonly PlayerSessionManager _playerSessionManager;

        private static readonly string ServiceUnavailableErrorCode = "ServiceUnavailable";

        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(GuestAccessService));

        public GuestAccessService()
        {
            _playerSessionManager = new PlayerSessionManager();
        }

        public PlayerSession AccessAsGuest()
        {
            try
            {
                PlayerSession guestSession = _playerSessionManager.CreateGuestSession();

                ApplicationLogger.LogInformation("Guest {0} accessed the game.", guestSession.Username);

                return guestSession;
            }
            catch (CryptographicException cryptographicException)
            {
                ApplicationLogger.LogError(cryptographicException,
                    "A cryptographic failure prevented guest access from completing.");

                throw CreateGuestAccessFault(ServiceUnavailableErrorCode);
            }
        }

        private FaultException<GuestAccessFault> CreateGuestAccessFault(string errorCode)
        {
            var guestAccessFault = new GuestAccessFault
            {
                ErrorCode = errorCode
            };

            var faultException =
                new FaultException<GuestAccessFault>(guestAccessFault);

            return faultException;
        }
    }
}