using Backgammon.Server.SupportServices;

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