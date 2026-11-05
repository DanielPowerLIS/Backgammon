using Backgammon.Server.SupportServices;
using Backgammon.Server.Services.Contracts;

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