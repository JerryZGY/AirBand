using System.Windows;

namespace AirBand
{
    public partial class App : Application
    {
        private void Application_Exit (object sender, ExitEventArgs e)
        {
            ( Switcher.PageDictionary["Playing"] as AirBand.Pages.Playing ).Dispose();
            ( Switcher.PageDictionary["Playing"] as AirBand.Pages.Playing ).OutDeviceDispose();
        }
    }
}