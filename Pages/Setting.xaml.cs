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
    /// Interaction logic for Setting.xaml
    /// </summary>
    public partial class Setting : UserControl
    {
        public Setting ()
        {
            InitializeComponent();
            MusicVolume.ValueChanged += Slider_ValueChanged;
        }

        private void Button_Click (object sender, RoutedEventArgs e)
        {
            this.IsHitTestVisible = false;
            StoryboardHandler.InitNotHitStoryBoard(Switcher.PageDictionary["MainMenu"], "ExitContentStoryboard", () =>
            {
                ( (Grid)Switcher.PageDictionary["MainMenu"].FindName("Grid_Main") ).IsHitTestVisible = true;
                this.IsHitTestVisible = true;
            });
        }

        private void Slider_ValueChanged (object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ( (MediaElement)Switcher.PageDictionary["MainMenu"].FindName("Music") ).Volume = e.NewValue / 100;
            MusicVolumeValue.Text = ( (int)e.NewValue ).ToString();
        }
    }
}
