using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using Microsoft.Kinect.Input;
using Microsoft.Kinect.Wpf.Controls;

namespace KinectAirBand.Pages
{
    /// <summary>
    /// Interaction logic for StartPlaying.xaml
    /// </summary>
    public partial class StartPlaying : UserControl
    {
        public ImageSource ImageSource
        {
            get
            {
                return _colorBitmap;
            }
        }
        private WriteableBitmap _colorBitmap = null;
        private Boolean Dashed = false;
        private Boolean Ensemble = false;
        private KinectSensor _sensor;
        private MultiSourceFrameReader _reader;
        //private Body[] _bodies;

        public StartPlaying ()
        {
            InitializeComponent();
            KinectCoreWindow kinectCoreWindow = KinectCoreWindow.GetForCurrentThread();
            kinectCoreWindow.PointerMoved += kinectCoreWindow_PointerMoved;
            Grid_Main.Opacity = 0;
            this.IsHitTestVisible = false;
            KinectRegion.SetKinectRegion(this, kinectRegion);
            _sensor = KinectSensor.GetDefault();
            kinectRegion.KinectSensor = _sensor;
            _sensor.Open();
            _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Body);
            _reader.MultiSourceFrameArrived += reader_MultiSourceFrameArrived;
            FrameDescription colorFrameDescription = _sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            _colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
        }

        private void kinectCoreWindow_PointerMoved (object sender, KinectPointerEventArgs e)
        {
            KinectPointerPoint kinectPointerPoint = e.CurrentPoint;
            
        }

        private void reader_MultiSourceFrameArrived (object sender, MultiSourceFrameArrivedEventArgs e)
        {
            MultiSourceFrame reference = e.FrameReference.AcquireFrame();
            using (ColorFrame colorFrame = reference.ColorFrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    FrameDescription colorFrameDescription = colorFrame.FrameDescription;
                    using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                    {
                        _colorBitmap.Lock();
                        if (( colorFrameDescription.Width == this._colorBitmap.PixelWidth ) && ( colorFrameDescription.Height == _colorBitmap.PixelHeight ))
                        {
                            colorFrame.CopyConvertedFrameDataToIntPtr(this._colorBitmap.BackBuffer, (uint)( colorFrameDescription.Width * colorFrameDescription.Height * 4 ), ColorImageFormat.Bgra);
                            _colorBitmap.AddDirtyRect(new Int32Rect(0, 0, _colorBitmap.PixelWidth, _colorBitmap.PixelHeight));
                        }
                        _colorBitmap.Unlock();
                        test.Source = _colorBitmap;
                    }
                }
            }
        }

        private void UserControl_Loaded (object sender, RoutedEventArgs e)
        {
            Storyboard storyBoard = ( (Storyboard)this.Resources["EnterStoryboard"] );
            storyBoard.Completed += (se, ev) => { this.IsHitTestVisible = true; };
            storyBoard.Begin();
        }

        private void Button_Back_Click (object sender, RoutedEventArgs e)
        {
            this.IsHitTestVisible = false;
            Storyboard storyBoard = ( (Storyboard)this.Resources["ExitStoryboard"] );
            storyBoard.Completed += (se, ev) => { Switcher.Switch(new MainMenu()); };
            storyBoard.Begin();
        }

        private void Button_Dashed_Click (object sender, RoutedEventArgs e)
        {
            var thisButton = sender as Button;
            thisButton.IsHitTestVisible = false;
            if (!Dashed)
            {
                Storyboard storyBoard = ( (Storyboard)this.Resources["DashStoryboard"] );
                storyBoard.Completed += (se, ev) => { thisButton.IsHitTestVisible = true; };
                storyBoard.Begin();
                Dashed = true;
            }
            else
            {
                Storyboard storyBoard = ( (Storyboard)this.Resources["DashedStoryboard"] );
                storyBoard.Completed += (se, ev) => { thisButton.IsHitTestVisible = true; };
                storyBoard.Begin();
                Dashed = false;
            }
        }

        private void Button_Ensemble_Click (object sender, RoutedEventArgs e)
        {
            var thisButton = sender as Button;
            thisButton.IsHitTestVisible = false;
            if (!Ensemble)
            {
                Storyboard storyBoard = ( (Storyboard)this.Resources["EnsembleStoryboard"] );
                storyBoard.Completed += (se, ev) => { thisButton.IsHitTestVisible = true; };
                storyBoard.Begin();
                Ensemble = true;
                Button_Ensemble.Content = "獨奏模式";
                Button_Ensemble.Background = new ImageBrush() { ImageSource =  ( (BitmapImage)this.Resources["Image_Single"] ) };
            }
            else
            {
                Storyboard storyBoard = ( (Storyboard)this.Resources["SingleStoryboard"] );
                storyBoard.Completed += (se, ev) => { thisButton.IsHitTestVisible = true; };
                storyBoard.Begin();
                Ensemble = false;
                Button_Ensemble.Content = "合奏模式";
                Button_Ensemble.Background = new ImageBrush() { ImageSource = ( (BitmapImage)this.Resources["Image_Ensemble"] ) };
            }
        }

    }
}