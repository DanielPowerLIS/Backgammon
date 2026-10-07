using System.Windows;

using Backgammon.Client.Localization;

namespace Backgammon.Client
{
	public partial class App : Application
	{
        protected override void OnStartup(StartupEventArgs e)
        {
            LocalizationService.Current.LoadPreferredLanguage();

            base.OnStartup(e);
        }
    }
}
