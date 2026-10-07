using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Windows;

using Backgammon.Client.Localization;
using Backgammon.Client.Services.Logging;

namespace Backgammon.Client
{
    public partial class App : Application
    {

        private static readonly string LoggingConfigurationFileName =
            "log4net.config";
        protected override void OnStartup(StartupEventArgs e)
        {
            ConfigureLogging();

            IApplicationLogger applicationLogger =
                new Log4NetApplicationLogger(typeof(App));

            applicationLogger.LogInformation("Application started.");

            LocalizationService.Current.LoadPreferredLanguage();

            base.OnStartup(e);
        }

        private void ConfigureLogging()
        {
            string configurationFilePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                LoggingConfigurationFileName);

            FileInfo configurationFile = new FileInfo(configurationFilePath);

            XmlConfigurator.Configure(
                LogManager.GetRepository(typeof(App).Assembly),
                configurationFile);
        }
    }
}
