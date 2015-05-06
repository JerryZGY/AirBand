using System;
using System.Windows;
using System.Windows.Controls;
using KinectAirBand.Pages;
using Microsoft.Kinect;
using Microsoft.Kinect.Wpf.Controls;

namespace KinectAirBand
{
    public partial class PageSwitcher : Window
    {
        public PageSwitcher ()
        {
            InitializeComponent();
            KinectRegion.SetKinectRegion(this, kinectRegion);
            App app = ( (App)Application.Current );
            app.KinectRegion = kinectRegion;
            kinectRegion.KinectSensor = KinectSensor.GetDefault();
            if(SystemParameters.FullPrimaryScreenWidth > 1366)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
            Switcher.pageSwitcher = this;
            Switcher.Switch(new MainMenu());
        }

        public void Navigate (UserControl nextPage)
        {
            kinectRegion.Content = nextPage;
        }

        public void Navigate (UserControl nextPage, object state)
        {
            kinectRegion.Content = nextPage;
            ISwitchable s = nextPage as ISwitchable;
            if (s != null)
                s.UtilizeState(state);
            else
                throw new ArgumentException("NextPage is not ISwitchable! " + nextPage.Name.ToString());
        }
    }
}