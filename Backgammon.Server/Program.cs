using System;

using Backgammon.Server.Services.Comunication;

namespace Backgammon.Server
{
    internal class Program
    {
        private static void Main()
        {
            var leaderboardServiceHost = new LeaderboardServiceHost();

            try
            {
                leaderboardServiceHost.Open();

                Console.WriteLine("Press ENTER to stop the server.");
                Console.ReadLine();
            }
            finally
            {
                leaderboardServiceHost.Close();
            }
        }
    }
}