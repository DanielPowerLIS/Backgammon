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
        private readonly PlayerSessionService _playerSessionService;

        private static readonly string ServiceUnavailableErrorCode =
            "ServiceUnavailable";

        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(GuestAccessService));

        public GuestAccessService()
        {
            _playerSessionService = new PlayerSessionService();
        }

        public PlayerSession AccessAsGuest()
        {
            try
            {
                PlayerSession guestSession =
                    _playerSessionService.CreateGuestSession();

                ApplicationLogger.LogInformation(
                    "Guest {0} accessed the game.", guestSession.Username);

                return guestSession;
            }
            catch (Exception guestException) when (
                guestException is CryptographicException ||
                guestException is InvalidOperationException)
            {
                ApplicationLogger.LogError(
                    guestException, "Guest access could not be completed.");

                throw CreateGuestAccessFault();
            }
        }

        private FaultException<GuestAccessFault> CreateGuestAccessFault()
        {
            var guestAccessFault = new GuestAccessFault
            {
                ErrorCode = ServiceUnavailableErrorCode
            };

            var faultException =
                new FaultException<GuestAccessFault>(guestAccessFault);

            return faultException;
        }
    }
}