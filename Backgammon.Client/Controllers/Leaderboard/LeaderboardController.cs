using System.Threading.Tasks;

using Backgammon.Client.Comunication;
using Backgammon.Client.LeaderboardReference;

namespace Backgammon.Client.Controllers.Leaderboard
{
    internal class LeaderboardController
    {
        private readonly LeaderboardClient _leaderboardClient;

        public LeaderboardController()
        {
            _leaderboardClient = new LeaderboardClient();
        }

        public Task<PlayerStanding[]> GetLeaderboardAsync()
        {
            Task<PlayerStanding[]> leaderboardTask =
                _leaderboardClient.GetLeaderboardAsync();

            return leaderboardTask;
        }
    }
}