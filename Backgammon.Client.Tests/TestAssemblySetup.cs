using System.IO;

using log4net;
using log4net.Config;
using log4net.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Backgammon.Client.Settings.Logging;

namespace Backgammon.Client.Tests
{
    [TestClass]
    public class TestAssemblySetup
    {
        private static readonly string LoggingConfigurationFileName =
            "log4net.Tests.config";

        [AssemblyInitialize]
        public static void Initialize(TestContext testContext)
        {
            string assemblyDirectory = Path.GetDirectoryName(
                typeof(TestAssemblySetup).Assembly.Location);

            string configurationFilePath = Path.Combine(
                assemblyDirectory, LoggingConfigurationFileName);

            var configurationFile = new FileInfo(configurationFilePath);

            Assert.IsTrue(
                configurationFile.Exists,
                "The test logging configuration file was not found.");

            ILoggerRepository loggingRepository = LogManager.GetRepository(
                typeof(Log4NetApplicationLogger).Assembly);

            loggingRepository.ResetConfiguration();
            XmlConfigurator.Configure(loggingRepository, configurationFile);

            Assert.IsTrue(
                loggingRepository.Configured,
                "Logging could not be configured for tests.");
        }
    }
}