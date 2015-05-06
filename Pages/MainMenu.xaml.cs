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
using Microsoft.Kinect;
using Microsoft.Kinect.Wpf.Controls;

namespace KinectAirBand.Pages
{
    public partial class MainMenu : UserControl, ISwitchable
    {
        public MainMenu ()
        {
            InitializeComponent();
            Grid_Main.Opacity = 0;
            this.IsHitTestVisible = false;
        }

        #region ISwitchable Members

        public void UtilizeState (object state)
        {
            throw new NotImplementedException();
        }

        #endregion

        private void UserControl_Loaded (object sender, RoutedEventArgs e)
        {
            Storyboard storyBoard = ((Storyboard)this.Resources["EnterStoryboard"]);
            storyBoard.Completed += (se, ev) => { this.IsHitTestVisible = true; };
            storyBoard.Begin();
        }

        private void StartExitStoryboard (Action callback)
        {
            this.IsHitTestVisible = false;
            Storyboard storyBoard = ( (Storyboard)this.Resources["ExitStoryboard"] );
            storyBoard.Completed += (se, ev) => callback();
            storyBoard.Begin();
        }

        private void Button_Click (object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            switch (button.Name)
            {
                case "Button_Start":
                    StartExitStoryboard(() => Switcher.Switch(new StartPlaying()));
                    break;
                case "Button_Share":
                    StartExitStoryboard(() => Switcher.Switch(new StartPlaying()));
                    break;
                case "Button_Mod":
                    StartExitStoryboard(() => Switcher.Switch(new StartPlaying()));
                    break;
                case "Button_Setting":
                    StartExitStoryboard(() => Switcher.Switch(new StartPlaying()));
                    break;
                case "Button_About":
                    StartExitStoryboard(() => Switcher.Switch(new StartPlaying()));
                    break;
                case "Button_Exit":
                    Application.Current.Shutdown();
                    break;
                default:
                    break;
            }
        }
    }
}