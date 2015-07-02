using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace AirBand.Controls
{
    /// <summary>
    /// Interaction logic for ImageListControl.xaml
    /// </summary>
    public partial class MaskListControl : UserControl
    {
        private Int32 index = 0;
        private String[] maskArray;
        public MaskListControl ()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded (object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                maskArray = System.IO.Directory.GetFileSystemEntries(@"Resources/Mask", "*.png").ToArray();
            }
        }

        private void Button_Click (object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            switch (button.Name)
            {
                case "Button_Prev":
                    index = ( index > 0 ) ? ( index - 1 ) : ( maskArray.Count() - 1 );
                    break;
                case "Button_Next":
                    index = ( index < maskArray.Count() - 1 ) ? ( index + 1 ) : 0;
                    break;
            }
            var src = new BitmapImage();
            src.BeginInit();
            src.UriSource = new Uri(maskArray[index], UriKind.Relative);
            src.CacheOption = BitmapCacheOption.OnLoad;
            src.EndInit();
            Image.Source = src;
            Switcher.viewModel.Mask = src;
        }
    }
}