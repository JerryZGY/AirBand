using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using KinectAirBand.Pages;
using Microsoft.Kinect;
using Microsoft.Kinect.Input;
using Microsoft.Kinect.Wpf.Controls;

namespace KinectAirBand
{
    public partial class PageSwitcher : Window
    {
        public Dictionary<Type, UserControl> PageDictionary = new Dictionary<Type,UserControl>();

        public PageSwitcher ()
        {
            InitializeComponent();
            KinectRegion.SetKinectRegion(this, kinectRegion);
            App app = ( (App)Application.Current );
            app.KinectRegion = kinectRegion;
            app.KinectCoreWindow = KinectCoreWindow.GetForCurrentThread();
            kinectRegion.KinectSensor = KinectSensor.GetDefault();
            if(SystemParameters.FullPrimaryScreenWidth > 1366)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
            Switcher.pageSwitcher = this;
            PageDictionary.Add(typeof(MainMenu), new MainMenu());
            Switcher.Switch(PageDictionary[typeof(MainMenu)]);
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