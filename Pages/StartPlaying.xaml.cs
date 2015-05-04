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
    /// <summary>
    /// Interaction logic for StartPlaying.xaml
    /// </summary>
    public partial class StartPlaying : UserControl
    {
        public StartPlaying ()
        {
            InitializeComponent();
            Grid_Main.Opacity = 0;
        }

        private void UserControl_Loaded (object sender, RoutedEventArgs e)
        {
            Storyboard storyBoard = ( (Storyboard)this.Resources["EnterStoryboard"] );
            storyBoard.Begin();
        }

        private void Button_Back_Click (object sender, RoutedEventArgs e)
        {
            Storyboard storyBoard = ( (Storyboard)this.Resources["ExitStoryboard"] );
            storyBoard.Completed += (se, ev) => { Switcher.Switch(new MainMenu()); };
            storyBoard.Begin();
        }
    }
}
