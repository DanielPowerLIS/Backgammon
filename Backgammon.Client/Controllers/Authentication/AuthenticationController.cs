using System.Threading.Tasks;

using Backgammon.Client.AuthenticationReference;
using Backgammon.Client.Comunication;
using Backgammon.Client.Controllers.Session;

namespace Backgammon.Client.Controllers.Authentication
{
    internal class AuthenticationController
    {
        private readonly AuthenticationClient _authenticationClient;

        public AuthenticationController()
        {
            _authenticationClient = new AuthenticationClient();
        }

        public async Task SignInAsync(
            string accountIdentifier, string suppliedPassword)
        {
            var playerCredentials = new Credentials
            {
                Identifier = accountIdentifier,
                Password = suppliedPassword
            };

            PlayerSession playerSession =
                await _authenticationClient.SignInAsync(playerCredentials);

            PlayerSessionContext.StoreSession(playerSession);
        }
    }
}
