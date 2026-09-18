using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Backgammon.Client
{
	/// <summary>
	/// Lógica de interacción para App.xaml
	/// </summary>
	public partial class App : Application
	{
        protected override void OnStartup(StartupEventArgs e)
        {
            CultureInfo culture = new CultureInfo("es");

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            MessageBox.Show(
                Backgammon.Client.Properties.Resources.Guest + "\n" +
                Thread.CurrentThread.CurrentUICulture.Name
            );

            base.OnStartup(e);
        }
    }
}
