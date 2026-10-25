using System;
using System.IO;

using log4net.Config;

using Backgammon.Server.Services.Comunication;
using Backgammon.Server.SupportServices.Logging;

namespace Backgammon.Server
{
    internal class Program
    {
        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(Program));

        private static void Main()
        {
            ConfigureLogging();
            ApplicationLogger.LogInformation("Server logging initialized.");

            var leaderboardServiceHost = new LeaderboardServiceHost();
            var authenticationServiceHost = new AuthenticationServiceHost();
            var logOutServiceHost = new LogOutServiceHost();

            try
            {
                leaderboardServiceHost.Open();
                authenticationServiceHost.Open();
                logOutServiceHost.Open();

                RunRegistrationService();
            }
            finally
            {
                CloseServiceHosts(leaderboardServiceHost, authenticationServiceHost, logOutServiceHost);
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

        private static void CloseServiceHosts(
            LeaderboardServiceHost leaderboardServiceHost,
            AuthenticationServiceHost authenticationServiceHost,
            LogOutServiceHost logOutServiceHost)
        {
            try
            {
                logOutServiceHost.Close();
            }
            finally
            {
                CloseServiceHosts(leaderboardServiceHost, authenticationServiceHost);
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

        private static void RunRegistrationService()
        {
            var registrationServiceHost = new RegistrationServiceHost();

            try
            {
                registrationServiceHost.Open();

                Console.WriteLine("Press ENTER to stop the server.");
                Console.ReadLine();
            }
            finally
            {
                registrationServiceHost.Close();
            }
        }
    }
}