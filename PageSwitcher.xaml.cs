using System;
using System.Windows;
using System.Windows.Controls;
using AirBand.Pages;

namespace AirBand
{
    public partial class PageSwitcher : Window
    {
        public PageSwitcher()
        {
            InitializeComponent();
            Switcher.pageSwitcher = this;
            Switcher.VM_EnvironmentVariables.FullScreen = !(SystemParameters.FullPrimaryScreenWidth > 1366);
            Switcher.VM_EnvironmentVariables.FullScreenToggleButtonEnabled = (SystemParameters.FullPrimaryScreenWidth > 1366);
            Switcher.Switch(new Page_Main());
        }

        public void Navigate(UserControl nextPage)
        {
            GC.Collect();
            var prevPage = Presenter.Content as ISwitchable;
            if (prevPage != null)
                prevPage.ExitStory(() => Presenter.Content = nextPage);
            else
                Presenter.Content = nextPage;
        }
    }
}