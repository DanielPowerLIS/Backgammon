using Backgammon.Server.SupportServices;
using Backgammon.Server.Services.Contracts;

namespace Backgammon.Server.Services
{
    public class LogOutService : ILogOutService
    {
        private readonly PlayerSessionService _playerSessionService;

        public LogOutService()
        {
            _playerSessionService = new PlayerSessionService();
        }

        public void LogOut(string sessionId)
        {
            _playerSessionService.RemoveSession(sessionId);
        }
    }
}