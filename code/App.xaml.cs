using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace XDCheckerRecode
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Set shutdown mode to explicit so we can handle window transitions
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var splash = new SplashWindow();
            splash.Show();
        }
    }
}
