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
using KinectAirBand.Controls;
using Microsoft.Kinect;
using Microsoft.Kinect.Input;
using Microsoft.Kinect.Wpf.Controls;
using Sanford.Multimedia.Midi;
using Sanford.Multimedia.Midi.UI;

namespace KinectAirBand.Pages
{
    /// <summary>
    /// Interaction logic for StartPlaying.xaml
    /// </summary>
    public partial class StartPlaying : UserControl, ISwitchable, IDisposable
    {
        public ImageSource ImageSource
        {
            get
            {
                return _colorBitmap;
            }
        }
        private WriteableBitmap _colorBitmap = null;
        private Boolean disposed;
        private Boolean dashed = false;
        private Boolean ensemble = false;
        private Boolean isLasso = false;
        private Boolean playing = false;
        private KinectRegion kinectRegion;
        private KinectSensor _sensor;
        private MultiSourceFrameReader _reader;
        private Body[] _bodies;
        private App app = Application.Current as App;
        private OutputDevice outDevice;
        private Int32 outDeviceID = 0;
        private KinectCoreWindow kinectCoreWindow;
        private FrameDescription colorFrameDescription;

        public StartPlaying ()
        {
            InitializeComponent();
        }

        ~StartPlaying ()
        {
            Dispose(false);
        }

        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose (bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                
            }
            outDevice.Dispose();
            _reader.Dispose();
            outDevice = null;
            /*kinectCoreWindow.PointerMoved -= kinectCoreWindow_PointerMoved;
            _colorBitmap = null;
            kinectRegion = null;
            _sensor = null;
            _bodies = null;
            app = null;
            kinectCoreWindow = null;
            _reader = null;
            colorFrameDescription = null;*/

            disposed = true;
        }

        #region ISwitchable Members

        public void UtilizeState (object state)
        {
            throw new NotImplementedException();
        }

        #endregion

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

            Boolean dataReceived = false;

            using (BodyFrame frame = reference.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (_bodies == null)
                    {
                        _bodies = new Body[frame.BodyCount];
                    }
                    frame.GetAndRefreshBodyData(_bodies);
                    dataReceived = true;
                }
            }

            if (dataReceived)
            {
                if (_bodies != null)
                {
                    for (int i = 0; i < _sensor.BodyFrameSource.BodyCount; ++i)
                    {
                        Body body = _bodies[i];
                        if (body.IsTracked)
                        {
                            isLasso = ( body.HandRightState == HandState.Lasso || body.HandLeftState == HandState.Lasso );
                        }
                    }
                }
            }

            reference = null;
        }

        private void UserControl_Loaded (object sender, RoutedEventArgs e)
        {
            Grid_Main.Opacity = 0;
            this.IsHitTestVisible = false;
            this.disposed = false;
            _sensor = app.KinectSensor;
            kinectRegion = app.KinectRegion;
            kinectCoreWindow = app.KinectCoreWindow;
            _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Body);
            _reader.MultiSourceFrameArrived += reader_MultiSourceFrameArrived;
            colorFrameDescription = _sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            _colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
            kinectCoreWindow.PointerMoved += kinectCoreWindow_PointerMoved;
            outDevice = new OutputDevice(outDeviceID);
            Storyboard storyBoard = ( (Storyboard)this.Resources["EnterStoryboard"] );
            storyBoard.Completed += (se, ev) => { this.IsHitTestVisible = true; storyBoard = null; };
            storyBoard.Begin();
        }

        private void kinectCoreWindow_PointerMoved (object sender, KinectPointerEventArgs args)
        {
            KinectPointerPoint kinectPointerPoint = args.CurrentPoint;
            if (kinectPointerPoint.Properties.IsEngaged)
            {
                Point pointRelativeToKinectRegion = new Point(
                    kinectPointerPoint.Position.X * kinectRegion.ActualWidth,
                    kinectPointerPoint.Position.Y * kinectRegion.ActualHeight);
                foreach (PianoKeyWPF key in PianoControl.cnvPiano.Children)
                {
                    performLassoClick(pointRelativeToKinectRegion, key);
                }
                performLassoClick(pointRelativeToKinectRegion, Button_Dashed);
                performLassoClick(pointRelativeToKinectRegion, Button_Ensemble);
                performLassoClick(pointRelativeToKinectRegion, Button_Piano);
                performLassoClick(pointRelativeToKinectRegion, Button_Guitar);
                performLassoClick(pointRelativeToKinectRegion, Button_Drum);
            }
        }

        private void performLassoClick (Point relative, UIElement relativeTo)
        {
            var clickedButton = relativeTo as Button;
            if (clickedButton != null)
            {
                Point relativeToElement = kinectRegion.TranslatePoint(relative, clickedButton);
                bool insideElement = ( relativeToElement.X >= 0 && relativeToElement.X < clickedButton.ActualWidth
                    && relativeToElement.Y >= 0 && relativeToElement.Y < clickedButton.ActualHeight );
                if (insideElement && isLasso && clickedButton.IsHitTestVisible)
                {
                    clickedButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    VisualStateManager.GoToState(clickedButton, "Pressed", true);
                }
            }
        }

        private void Button_Back_Click (object sender, RoutedEventArgs e)
        {
            this.IsHitTestVisible = false;
            Dispose(true);
            Storyboard storyBoard = ( (Storyboard)this.Resources["ExitStoryboard"] );
            //storyBoard.Completed += (se, ev) => { Switcher.Switch(Switcher.pageSwitcher.PageDictionary[typeof(MainMenu)]); storyBoard = null; };
            storyBoard.Completed += (se, ev) => { Switcher.Switch(new MainMenu()); storyBoard = null; };
            storyBoard.Begin();
        }

        private void Button_Dashed_Click (object sender, RoutedEventArgs e)
        {
            var thisButton = sender as Button;
            thisButton.IsHitTestVisible = false;
            if (!dashed)
            {
                Storyboard storyBoard = ( (Storyboard)this.Resources["DashStoryboard"] );
                storyBoard.Completed += (se, ev) => { thisButton.IsHitTestVisible = true; };
                storyBoard.Begin();
                dashed = true;
            }
            else
            {
                Storyboard storyBoard = ( (Storyboard)this.Resources["DashedStoryboard"] );
                storyBoard.Completed += (se, ev) => { thisButton.IsHitTestVisible = true; };
                storyBoard.Begin();
                dashed = false;
            }
        }

        private void Button_Ensemble_Click (object sender, RoutedEventArgs e)
        {
            var thisButton = sender as Button;
            thisButton.IsHitTestVisible = false;
            if (!ensemble)
            {
                Storyboard storyBoard = ( (Storyboard)this.Resources["EnsembleStoryboard"] );
                storyBoard.Completed += (se, ev) => { thisButton.IsHitTestVisible = true; };
                storyBoard.Begin();
                ensemble = true;
                Button_Ensemble.Content = "獨奏模式";
                Button_Ensemble.Background = new ImageBrush() { ImageSource =  ( (BitmapImage)this.Resources["Image_Single"] ) };
            }
            else
            {
                Storyboard storyBoard = ( (Storyboard)this.Resources["SingleStoryboard"] );
                storyBoard.Completed += (se, ev) => { thisButton.IsHitTestVisible = true; };
                storyBoard.Begin();
                ensemble = false;
                Button_Ensemble.Content = "合奏模式";
                Button_Ensemble.Background = new ImageBrush() { ImageSource = ( (BitmapImage)this.Resources["Image_Ensemble"] ) };
            }
        }

        private void PianoControl_PianoKeyDown (object sender, PianoKeyEventArgs e)
        {
            #region Guard

            if (playing)
            {
                return;
            }

            #endregion

            outDevice.Send(new ChannelMessage(ChannelCommand.NoteOn, 0, e.NoteID, 127));
        }

        private void PianoControl_PianoKeyUp (object sender, PianoKeyEventArgs e)
        {
            #region Guard

            if (playing)
            {
                return;
            }

            #endregion

            outDevice.Send(new ChannelMessage(ChannelCommand.NoteOff, 0, e.NoteID, 0));
        }

    }
}