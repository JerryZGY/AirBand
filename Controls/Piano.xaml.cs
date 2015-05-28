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
using Microsoft.Kinect;

namespace KinectAirBand.Controls
{
    /// <summary>
    /// Piano.xaml 的互動邏輯
    /// </summary>
    public partial class Piano : UserControl
    {
        private const Double KeysHeight = 255;
        private const Double KeysWidth = 48.78;

        public Piano ()
        {
            InitializeComponent();
        }

        public void CreateKeys (JointType jointType, Point jointPoint)
        {
            var a = new Rectangle() { Fill = Brushes.White, Width = 48.78, Height = 255 };
            Canvas_Main.Children.Add(a);
        }
    }
}
