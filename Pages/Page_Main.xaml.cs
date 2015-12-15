using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using AirBand.Controls;

namespace AirBand.Pages
{
    public partial class Page_Main : UserControl, ISwitchable
    {
        public Page_Main()
        {
            InitializeComponent();
            DataContext = Switcher.VM_EnvironmentVariables;
        }

        public void EnterStory()
        {
            Storyboard story = ((Storyboard)Resources["Enter"]);
            story.Begin();
        }

        public void ExitStory(Action callback)
        {
            Storyboard story = ((Storyboard)Resources["Exit"]);
            story.Completed += (s, e) => callback();
            story.Begin();
        }

        public void InitializeProperty()
        {
            Grid_Main.Opacity = 0;
            Img_Logo.Opacity = 0;
            Img_Version.Opacity = 0;
            Img_Version.RenderTransform = new TranslateTransform(165, 110);
            Cnv_Btns.Opacity = 0;
            Cnv_Btns.RenderTransform = new RotateTransform(-90);
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
                case "Btn_Playing":
                    Switcher.Switch(new Page_Main());
                    break;
                case "Btn_About":
                    Presenter.Content = new Ctrl_About();
                    ((Storyboard)Resources["EnterContent"]).Begin();
                    Grid_Menu.IsHitTestVisible = false;
                    break;
                case "Btn_Options":
                    Presenter.Content = new Ctrl_Options();
                    ((Storyboard)Resources["EnterContent"]).Begin();
                    Grid_Menu.IsHitTestVisible = false;
                    break;
                case "Btn_Exit":
                    Application.Current.Shutdown();
                    break;
                case "Btn_Check":
                    Storyboard storya = ((Storyboard)Resources["ExitContent"]);
                    storya.Begin();
                    Grid_Menu.IsHitTestVisible = true;
                    break;
                default:
                    break;
            }
        }
    }
}