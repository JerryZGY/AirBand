﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
            InputBindings.Add(new InputBinding(new RightClickCommand(() => exitContent()), new MouseGesture(MouseAction.RightClick)));
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
            Inst.Opacity = 0;
            Inst.Width = 50;
            Inst.Height = 50;
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
                case "Btn_Playing":
                    Grid_Menu.IsHitTestVisible = false;
                    Switcher.Switch(new Page_Main());
                    break;
                case "Btn_Community":
                    Presenter.Content = new Ctrl_Community();
                    enterContent();
                    break;
                case "Btn_Mod":
                    Presenter.Content = new Ctrl_Mod();
                    enterContent();
                    break;
                case "Btn_Options":
                    Presenter.Content = new Ctrl_Options();
                    enterContent();
                    break;
                case "Btn_About":
                    Presenter.Content = new Ctrl_About();
                    enterContent();
                    break;
                case "Btn_Check":
                    exitContent();
                    break;
                case "Btn_Exit":
                    Application.Current.Shutdown();
                    break;
                default:
                    break;
            }
        }

        private void enterContent()
        {
            Grid_Menu.IsHitTestVisible = false;
            Switcher.VM_EnvironmentVariables.ContentEntered = true;
            this.Begin("EnterContent");
        }

        private void exitContent()
        {
            Grid_Menu.IsHitTestVisible = true;
            Switcher.VM_EnvironmentVariables.ContentEntered = false;
            this.Begin("ExitContent");
        }
    }
}