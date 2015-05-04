using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KinectAirBand.Pages
{
    public partial class MainMenu : UserControl, ISwitchable
    {
        public MainMenu ()
        {
            InitializeComponent();
            Grid_Main.Opacity = 0;
        }

        #region ISwitchable Members

        public void UtilizeState (object state)
        {
            throw new NotImplementedException();
        }

        #endregion

        private void Button_Click (object sender, RoutedEventArgs e)
        {
            Switcher.Switch(new Playing());
        }

        private void UserControl_Loaded (object sender, RoutedEventArgs e)
        {
            Storyboard storyBoard = ((Storyboard)this.Resources["EnterStoryboard"]);
            storyBoard.Begin();
        }

        private void Button_Exit_Click (object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Button_Start_Click (object sender, RoutedEventArgs e)
        {
            Storyboard storyBoard = ( (Storyboard)this.Resources["ExitStoryboard"] );
            storyBoard.Completed += (se, ev) => { Switcher.Switch(new StartPlaying()); };
            storyBoard.Begin();
        }

        private void Button_2_Click (object sender, RoutedEventArgs e)
        {
            Storyboard storyBoard = ( (Storyboard)this.Resources["SettingStoryboard"] );
            storyBoard.Begin();
        }
    }
}