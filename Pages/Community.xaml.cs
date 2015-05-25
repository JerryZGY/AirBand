﻿using System;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KinectAirBand.Pages
{
    /// <summary>
    /// Community.xaml 的互動邏輯
    /// </summary>
    public partial class Community : UserControl
    {
        public Community (EnvironmentVariablesViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }

        private void Button_Click (object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            switch (button.Name)
            {
                case "Button_Check":
                    this.IsHitTestVisible = false;
                    StoryboardHandler.InitNotHitStoryBoard(Switcher.PageDictionary["MainMenu"], "ExitContentStoryboard", () =>
                    {
                        ( (Grid)Switcher.PageDictionary["MainMenu"].FindName("Grid_Main") ).IsHitTestVisible = true;
                        Switcher.viewModel.ContentEntered = false;
                        this.IsHitTestVisible = true;
                    });
                    break;
                default:
                    break;
            }
        }
    }
}