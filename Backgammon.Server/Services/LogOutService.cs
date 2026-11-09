using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Services.Manager;

namespace Backgammon.Server.Services
{
    public class LogOutService : ILogOutService
    {
        private readonly PlayerSessionManager _playerSessionManager;

        public LogOutService()
        {
            _playerSessionManager = new PlayerSessionManager();
        }

        public void LogOut(string sessionId)
        {
            _playerSessionManager.RemoveSession(sessionId);
        }
    }
}
