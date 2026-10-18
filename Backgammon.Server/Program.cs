using System;
using System.IO;

using log4net.Config;
using Backgammon.Server.Services.Comunication;

namespace Backgammon.Server
{
    internal class Program
    {
        private static void Main()
        {
            ConfigureLogging();
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

        private static void ConfigureLogging()
        {
            string configurationPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "log4net.config");

            var configurationFile = new FileInfo(configurationPath);

            XmlConfigurator.Configure(configurationFile);
        }
    }
}