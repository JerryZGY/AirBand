using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace AirBand.Pages
{
    public partial class Page_Main : UserControl, ISwitchable
    {
        public Page_Main()
        {
            InitializeComponent();
        }

        public void EnterStory()
        {
            Storyboard story = ((Storyboard)Resources["Enter"]);
            story.Begin();
        }

        public void ExitStory(Action callback)
        {
            callback();
        }

        public void InitializeProperty()
        {
            Grid_Main.Opacity = 0;
            Img_Logo.Opacity = 0;
            Img_Version.Opacity = 0;
            Img_Version.RenderTransform = new TranslateTransform(165, 110);
            Cnv_Btns.Opacity = 0;
            Cnv_Btns.RenderTransform = new RotateTransform(-90);
            Cnv_Btns.InitializeProperty();
        }

        private void page_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeProperty();
            EnterStory();
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            switch (((Button)sender).Name)
            {
                //case "Button_Start":
                //    if (!Switcher.viewModel.BackgroundRemoval)
                //        StoryboardHandler.ExitStoryBoard(this, Image_LogoEffect, () => Switcher.Switch("Playing"));
                //    else
                //        StoryboardHandler.ExitStoryBoard(this, Image_LogoEffect, () => Switcher.Switch("BRPlaying"));
                //    break;
                //case "Button_Community":
                //    EnterContent("Community");
                //    break;
                //case "Button_Mod":
                //    EnterContent("Mod");
                //    break;
                //case "Button_Setting":
                //    EnterContent("Setting");
                //    break;
                //case "Btn_About":
                //    Switcher.Switch(new Page_Main());
                //    break;
                case "Btn_Exit":
                    Application.Current.Shutdown();
                    break;
                default:
                    break;
            }
        }
    }
}