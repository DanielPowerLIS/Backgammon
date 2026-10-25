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

            try
            {
                RunServer();
            }
            catch (Exception serverException)
            {
                ApplicationLogger.LogFatal(
                    serverException,
                    "An unrecoverable failure prevented the server from continuing.");

                throw;
            }
        }

        private static void RunServer()
        {
            var leaderboardServiceHost = new LeaderboardServiceHost();
            var authenticationServiceHost = new AuthenticationServiceHost();
            var logOutServiceHost = new LogOutServiceHost();
            var profileServiceHost = new ProfileServiceHost();

            try
            {
                leaderboardServiceHost.Open();
                authenticationServiceHost.Open();
                logOutServiceHost.Open();
                profileServiceHost.Open();

                RunRegistrationService();
            }
            finally
            {
                CloseServiceHosts(new Action[]
                    {
                        profileServiceHost.Close,
                        logOutServiceHost.Close,
                        authenticationServiceHost.Close,
                        leaderboardServiceHost.Close
                    });
            }
        }

        private static void CloseServiceHosts(
            Action[] closeOperations, int operationIndex = 0)
        {
            if (operationIndex >= closeOperations.Length)
            {
                return;
            }

            try
            {
                closeOperations[operationIndex]();
            }
            finally
            {
                CloseServiceHosts(closeOperations, operationIndex + 1);
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