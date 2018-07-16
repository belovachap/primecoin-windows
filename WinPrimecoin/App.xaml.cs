using System;
using System.Windows;

namespace WinPrimecoin
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Boolean useTestnet = false;
            foreach (String argument in e.Args)
            {
                switch (argument)
                {
                    case "testnet":
                        useTestnet = true;
                        break;
                    default:
                        break;
                }
            }
            MainWindow mainWindow = new MainWindow(useTestnet);
            mainWindow.Show();
        }
    }
}
