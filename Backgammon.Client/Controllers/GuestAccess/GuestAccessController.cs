using System.Threading.Tasks;

using Backgammon.Client.Comunication;
using Backgammon.Client.Controllers.Session;
using GuestPlayerSession = Backgammon.Client.GuestAccessReference.PlayerSession;
using PlayerSession = Backgammon.Client.AuthenticationReference.PlayerSession;

namespace Backgammon.Client.Controllers.GuestAccess
{
    internal class GuestAccessController
    {
        private readonly GuestAccessClient _guestAccessClient;

        public GuestAccessController()
        {
            _guestAccessClient = new GuestAccessClient();
        }

        public async Task AccessAsGuestAsync()
        {
            GuestPlayerSession guestSession =
                await _guestAccessClient.AccessAsGuestAsync();

            PlayerSession playerSession = CreatePlayerSession(guestSession);

            PlayerSessionContext.StoreSession(playerSession);
        }

        private PlayerSession CreatePlayerSession(GuestPlayerSession guestSession)
        {
            var playerSession = new PlayerSession
            {
                SessionId = guestSession.SessionId,
                PlayerId = guestSession.PlayerId,
                Username = guestSession.Username,
                IsGuest = guestSession.IsGuest,
                ExpiresAtUtc = guestSession.ExpiresAtUtc
            };

            return playerSession;
        }
    }
}