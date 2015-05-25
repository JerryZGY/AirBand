using System;
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

namespace KinectAirBand.Controls
{
    /// <summary>
    /// Interaction logic for ImageListControl.xaml
    /// </summary>
    public partial class ImageListControl : UserControl
    {
        private Int32 index = 0;
        private String[] backgroundArray;
        public ImageListControl ()
        {
            InitializeComponent();
            backgroundArray = System.IO.Directory.GetFiles(@"Resources\Background", "*.jpg").Select(x => { return String.Format(@"..\{0}", x); }).ToArray();
        }

        private void Button_Prev_Click (object sender, RoutedEventArgs e)
        {
            index = ( index > 0 ) ? ( index - 1 ) : ( backgroundArray.Count() - 1 );
            Image.Source = new BitmapImage(new Uri(backgroundArray[index], UriKind.Relative));
        }

        private void Button_Next_Click (object sender, RoutedEventArgs e)
        {
            index = ( index < backgroundArray.Count() - 1 ) ? ( index + 1 ) : 0;
            Image.Source = new BitmapImage(new Uri(backgroundArray[index], UriKind.Relative));
        }
    }
}
