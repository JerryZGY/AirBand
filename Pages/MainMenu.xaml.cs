using System.Windows;
using System.Windows.Controls;

namespace KinectAirBand.Pages
{
    public partial class MainMenu : UserControl
    {
        public MainMenu ()
        {
            InitializeComponent();
            StoryboardHandler.InitStoryBoard(this, "LogoEffectStoryboard");
        }

        private void MainMenu_Loaded (object sender, RoutedEventArgs e)
        {
            Grid_Main.Opacity = 0;
            StoryboardHandler.EnterStoryBoard(this, Image_LogoEffect);
        }

        private void Button_Click (object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            switch (button.Name)
            {
                case "Button_Start":
                    StoryboardHandler.ExitStoryBoard(this, Image_LogoEffect, () => Switcher.Switch("StartPlaying"));
                    break;
                case "Button_Share":
                    break;
                case "Button_Mod":
                    break;
                case "Button_Setting":
                    break;
                case "Button_About":
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