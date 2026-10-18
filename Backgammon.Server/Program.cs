using System;
using System.IO;

using log4net.Config;
using Backgammon.Server.Services.Comunication;
using Backgammon.Server.SupportServices.Logging;

namespace Backgammon.Server
{
    internal class Program
    {
        private static void Main()
        {
            ConfigureLogging();
            var applicationLogger = new Log4NetApplicationLogger(typeof(Program));
            applicationLogger.LogInformation("Server logging initialized.");

            var leaderboardServiceHost = new LeaderboardServiceHost();
            var authenticationServiceHost = new AuthenticationServiceHost();

            try
            {
                leaderboardServiceHost.Open();
                authenticationServiceHost.Open();

                Console.WriteLine("Press ENTER to stop the server.");
                Console.ReadLine();
            }
            finally
            {
                CloseServiceHosts(leaderboardServiceHost, authenticationServiceHost);
            }
        }

        private static void CloseServiceHosts(LeaderboardServiceHost leaderboardServiceHost,
            AuthenticationServiceHost authenticationServiceHost)
        {
            try
            {
                authenticationServiceHost.Close();
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