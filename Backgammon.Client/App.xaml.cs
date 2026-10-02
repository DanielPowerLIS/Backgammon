using System.Globalization;
using System.Threading;
using System.Windows;

namespace Backgammon.Client
{
	public partial class App : Application
	{
        protected override void OnStartup(StartupEventArgs e)
        {
            CultureInfo culture = new CultureInfo("en-US");

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            base.OnStartup(e);
        }
    }
}
