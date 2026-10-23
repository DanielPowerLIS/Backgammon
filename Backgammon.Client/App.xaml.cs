using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Windows;

using Backgammon.Client.Settings.Localization;
using Backgammon.Client.Settings.Logging;

namespace Backgammon.Client
{
    public partial class App : Application
    {
        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(App));

        private static readonly string LoggingConfigurationFileName =
            "log4net.config";

        protected override void OnStartup(StartupEventArgs e)
        {
            ConfigureLogging();

            ApplicationLogger.LogInformation("Application started.");

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
