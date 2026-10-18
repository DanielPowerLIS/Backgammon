using System.Threading.Tasks;

using Backgammon.Client.AuthenticationReference;
using Backgammon.Client.Comunication;

namespace Backgammon.Client.Controllers.Session
{
    internal class LogOutController
    {
        private readonly LogOutClient _logOutClient;

        public LogOutController()
        {
            _logOutClient = new LogOutClient();
        }

        public async Task LogOutAsync()
        {
            PlayerSession playerSession = PlayerSessionContext.GetSession();

            try
            {
                await _logOutClient.LogOutAsync(playerSession.SessionId);
            }
            finally
            {
                PlayerSessionContext.ClearSession();
            }
        }
    }
}
