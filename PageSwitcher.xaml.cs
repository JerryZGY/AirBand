using System;
using System.Windows;
using System.Windows.Controls;

namespace KinectAirBand
{
    public partial class PageSwitcher : Window
    {
        public PageSwitcher ()
        {
            InitializeComponent();

            if(SystemParameters.FullPrimaryScreenWidth > 1366)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;

            Switcher.pageSwitcher = this;
            Switcher.Switch("MainMenu");
        }

        public void Navigate (UserControl nextPage)
        {
            this.Content = nextPage;
        }

        public void Navigate (UserControl nextPage, object state)
        {
            this.Content = nextPage;
            ISwitchable s = nextPage as ISwitchable;
            if (s != null)
                s.UtilizeState(state);
            else
                throw new ArgumentException("NextPage is not ISwitchable! " + nextPage.Name.ToString());
        }
    }
}