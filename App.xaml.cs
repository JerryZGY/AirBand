using System.Windows;

namespace KinectAirBand
{
    public partial class App : Application
    {
        private void Application_Exit (object sender, ExitEventArgs e)
        {
            ( Switcher.PageDictionary["StartPlaying"] as KinectAirBand.Pages.StartPlaying ).Dispose();
        }
    }
}