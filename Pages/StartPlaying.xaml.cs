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
        Boolean Dashed = false;
        Boolean Ensemble = false; 

        public StartPlaying ()
        {
            InitializeComponent();
            Grid_Main.Opacity = 0;
            this.IsHitTestVisible = false;
        }

        private void UserControl_Loaded (object sender, RoutedEventArgs e)
        {
            Storyboard storyBoard = ( (Storyboard)this.Resources["EnterStoryboard"] );
            storyBoard.Completed += (se, ev) => { this.IsHitTestVisible = true; };
            storyBoard.Begin();
        }

        private void Button_Back_Click (object sender, RoutedEventArgs e)
        {
            this.IsHitTestVisible = false;
            Storyboard storyBoard = ( (Storyboard)this.Resources["ExitStoryboard"] );
            storyBoard.Completed += (se, ev) => { Switcher.Switch(new MainMenu()); };
            storyBoard.Begin();
        }

        private void Button_Dashed_Click (object sender, RoutedEventArgs e)
        {
            var thisButton = sender as Button;
            thisButton.IsHitTestVisible = false;
            if (!Dashed)
            {
                Storyboard storyBoard = ( (Storyboard)this.Resources["DashStoryboard"] );
                storyBoard.Completed += (se, ev) => { thisButton.IsHitTestVisible = true; };
                storyBoard.Begin();
                Dashed = true;
            }
            else
            {
                Storyboard storyBoard = ( (Storyboard)this.Resources["DashedStoryboard"] );
                storyBoard.Completed += (se, ev) => { thisButton.IsHitTestVisible = true; };
                storyBoard.Begin();
                Dashed = false;
            }
        }

        private void Button_Ensemble_Click (object sender, RoutedEventArgs e)
        {
            var thisButton = sender as Button;
            thisButton.IsHitTestVisible = false;
            if (!Ensemble)
            {
                Storyboard storyBoard = ( (Storyboard)this.Resources["EnsembleStoryboard"] );
                storyBoard.Completed += (se, ev) => { thisButton.IsHitTestVisible = true; };
                storyBoard.Begin();
                Ensemble = true;
                Button_Ensemble.Content = "獨奏模式";
                Button_Ensemble.Background = new ImageBrush() { ImageSource =  ( (BitmapImage)this.Resources["Image_Single"] ) };
            }
            else
            {
                Storyboard storyBoard = ( (Storyboard)this.Resources["SingleStoryboard"] );
                storyBoard.Completed += (se, ev) => { thisButton.IsHitTestVisible = true; };
                storyBoard.Begin();
                Ensemble = false;
                Button_Ensemble.Content = "合奏模式";
                Button_Ensemble.Background = new ImageBrush() { ImageSource = ( (BitmapImage)this.Resources["Image_Ensemble"] ) };
            }
        }
    }
}