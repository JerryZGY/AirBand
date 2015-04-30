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
            Storyboard storyBoard = (Storyboard)this.Resources["StartStoryboard"];
            Storyboard.SetTarget(storyBoard.Children.ElementAt(0) as DoubleAnimation, Grid_Main);
            Storyboard.SetTarget(storyBoard.Children.ElementAt(1) as DoubleAnimation, Button_2);
            Storyboard.SetTarget(storyBoard.Children.ElementAt(2) as DoubleAnimation, Button_1);
            Storyboard.SetTarget(storyBoard.Children.ElementAt(3) as DoubleAnimation, Button_3);
            Storyboard.SetTarget(storyBoard.Children.ElementAt(4) as DoubleAnimation, Button_0);
            Storyboard.SetTarget(storyBoard.Children.ElementAt(5) as DoubleAnimation, Button_4);
            Storyboard.SetTarget(storyBoard.Children.ElementAt(6) as DoubleAnimation, Button_Exit);
            storyBoard.Begin();
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
            
        }

        private void Button_Exit_Click (object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

    }
}