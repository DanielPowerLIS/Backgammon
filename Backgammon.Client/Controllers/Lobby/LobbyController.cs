using Backgammon.Client.Controllers.Session;

namespace Backgammon.Client.Controllers.Lobby
{
    internal class LobbyController
    {
        public string GetCurrentPlayerUsername()
        {
            string playerUsername = PlayerSessionContext.GetSession().Username;

            return playerUsername;
        }
    }
}