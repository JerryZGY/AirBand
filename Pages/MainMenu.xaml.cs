using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KinectAirBand.Pages
{
    public partial class MainMenu : UserControl
    {

        public MainMenu (EnvironmentVariablesViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
            StoryboardHandler.InitStoryBoard(this, "BackgroundEffectStoryboard");
            StoryboardHandler.InitStoryBoard(this, "LogoEffectStoryboard");
        }

        private void loaded (object sender, RoutedEventArgs e)
        {
            Grid_Main.Opacity = 0;
            StoryboardHandler.InitHitStoryBoard(this, "EnterStoryboard", () =>
            {
                Image_LogoEffect.Visibility = System.Windows.Visibility.Visible;
                Music.Play();
            });
        }

        private void Button_Click (object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            switch (button.Name)
            {
                case "Button_Start":
                    StoryboardHandler.ExitStoryBoard(this, Image_LogoEffect, () => Switcher.Switch("StartPlaying"));
                    break;
                case "Button_Community":
                    EnterContent("Community");
                    break;
                case "Button_Mod":
                    EnterContent("Mod");
                    break;
                case "Button_Setting":
                    EnterContent("Setting");
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

        private void EnterContent (String contentName)
        {
            StoryboardHandler.InitNotHitStoryBoard(this, Grid_Main, "EnterContentStoryboard", () =>
            {
                StackPanel_Content.Visibility = System.Windows.Visibility.Visible;
                StackPanel_Content.Children.Clear();
                StackPanel_Content.Children.Add(Switcher.PageDictionary[contentName]);
                Switcher.viewModel.ContentEntered = true;
            });
        }

        private void Music_MediaEnded (object sender, RoutedEventArgs e)
        {
            Music.Position = TimeSpan.Zero;
            Music.Play();
        }

        private void previewKeyDown (object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
            }
        }
    }
}