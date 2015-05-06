using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Kinect;
using Microsoft.Kinect.Wpf.Controls;

namespace KinectAirBand
{
    /// <summary>
    /// App.xaml 的互動邏輯
    /// </summary>
    public partial class App : Application
    {
        public KinectRegion KinectRegion { get; set; }
        public KinectSensor KinectSensor
        {
            get
            {
                return KinectRegion.KinectSensor;
            }
        }
    }
}