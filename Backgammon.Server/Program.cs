using System.Threading.Tasks;

using Backgammon.Server.Services.Comunication;

namespace Backgammon.Server
{
    internal class Program
    {
        private static async Task Main()
        {
            var leaderboardServiceHost = new LeaderboardServiceHost();

            await leaderboardServiceHost.RunAsync();
        }
    }
}