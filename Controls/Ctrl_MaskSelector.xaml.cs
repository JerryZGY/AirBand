using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace AirBand.Controls
{
    public partial class Ctrl_MaskSelector : UserControl
    {
        private int index = Switcher.VM_EnvironmentVariables.MaskIndex;
        private string[] maskArray;

        public Ctrl_MaskSelector()
        {
            InitializeComponent();
        }

        private void ctrl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
                maskArray = Directory.GetFileSystemEntries(@"Materials/Masks", "*.png").ToArray();
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            switch (button.Name)
            {
                case "Btn_Prev":
                    index = (index > 0) ? (index - 1) : (maskArray.Count() - 1);
                    break;
                case "Btn_Next":
                    index = (index < maskArray.Count() - 1) ? (index + 1) : 0;
                    break;
            }
            var src = new BitmapImage();
            src.BeginInit();
            src.UriSource = new Uri(maskArray[index], UriKind.Relative);
            src.CacheOption = BitmapCacheOption.OnLoad;
            src.EndInit();
            Img.Source = src;
            Switcher.VM_EnvironmentVariables.Mask = src;
            Switcher.VM_EnvironmentVariables.MaskIndex = index;
        }
    }
}
