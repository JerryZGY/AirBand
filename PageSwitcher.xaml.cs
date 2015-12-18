using System;
using System.Windows;
using System.Windows.Controls;
using AirBand.Pages;

namespace AirBand
{
    public partial class PageSwitcher : Window
    {
        public KinectHandler KinectHandler;
        public MidiHandler MidiHandler;
        public MyoHandler MyoHandler;

        public PageSwitcher()
        {
            KinectHandler = new KinectHandler();
            KinectHandler.InputEvent += inputEvent;
            MidiHandler = new MidiHandler();
            MyoHandler = new MyoHandler();
            DataContext = Switcher.VM_EnvironmentVariables;
            InitializeComponent();
            Switcher.PageSwitcher = this;
            Switcher.VM_EnvironmentVariables.FullScreen = !(SystemParameters.FullPrimaryScreenWidth > 1366);
            Switcher.VM_EnvironmentVariables.FullScreenToggleButtonEnabled = (SystemParameters.FullPrimaryScreenWidth > 1366);
            Switcher.Switch(new Page_Main());
            Music.Play();
        }

        private void mediaEnded(object sender, RoutedEventArgs e)
        {
            Music.Position = TimeSpan.Zero;
            Music.Play();
        }

        private void inputEvent(KinectInputArgs e)
        {
            if (e.IsValid)
            {
                Cur.Visibility = Visibility.Visible;
                Canvas.SetLeft(Cur, e.Posotion.X);
                Canvas.SetTop(Cur, e.Posotion.Y);
                if (e.InputState == InputState.Open)
                    Cur.Down();
                else
                    Cur.Up();
            }
            else
                Cur.Visibility = Visibility.Collapsed;
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