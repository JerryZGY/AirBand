using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace AirBand.Controls
{
    public partial class Ctrl_BackgroundSelector : UserControl
    {
        private int index = Switcher.VM_EnvironmentVariables.BackgroundIndex;
        private string[] backgroundArray;

        public Ctrl_BackgroundSelector()
        {
            InitializeComponent();
        }

        private void ctrl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                backgroundArray = Directory.GetFileSystemEntries(@"Materials/Backgrounds", "*.jpg").ToArray();
            }
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            switch (button.Name)
            {
                case "Btn_Prev":
                    index = (index > 0) ? (index - 1) : (backgroundArray.Count() - 1);
                    break;
                case "Btn_Next":
                    index = (index < backgroundArray.Count() - 1) ? (index + 1) : 0;
                    break;
            }
            var src = new BitmapImage();
            src.BeginInit();
            src.UriSource = new Uri(backgroundArray[index], UriKind.Relative);
            src.CacheOption = BitmapCacheOption.OnLoad;
            src.EndInit();
            Img.Source = src;
            Switcher.VM_EnvironmentVariables.Background = src;
            Switcher.VM_EnvironmentVariables.BackgroundIndex = index;
        }
    }
}
