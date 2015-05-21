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
    public partial class StartPlaying : UserControl, IDisposable
    {
        public ImageSource ImageSource
        {
            get
            {
                return colorBitmap;
            }
        }
        private WriteableBitmap colorBitmap = null;
        private Boolean disposed;
        private Boolean isLoaded = false;
        private Boolean isEnsembleMode;
        private Boolean playing;
        private KinectSensor sensor;
        private KinectCoreWindow kinectCoreWindow;
        private MultiSourceFrameReader reader;
        private Body[] bodies;
        private OutputDevice outDevice;
        private Int32 outDeviceID;
        private FrameDescription colorFrameDescription;
        private TimeSpan lastTime;
        private const double dotHeight = 30, dotWidth = 30;
        private List<PianoControlData> dataList;
        private Int32 trackingBodyIndex;
        private PianoControlData[] trackingData;
        private List<ToneTriggerHandler> toneTriggerList;
        private DrawingGroup drawingGroup;
        private DrawingImage imageSource;

        public StartPlaying (EnvironmentVariablesViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }

        ~StartPlaying ()
        {
            Dispose(false);
        }

        #region IDisposable

        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose (bool disposing)
        {
            if (disposed)
                return;

            if (isLoaded)
            {
                if (disposing && isLoaded)
                {
                    outDevice.Close();
                    outDevice.Dispose();
                    reader.Dispose();
                }

                //kinectCoreWindow.PointerMoved -= kinectCoreWindow_PointerMoved;
                colorBitmap = null;
                dataList = null;
                trackingData = null;
                sensor = null;
                bodies = null;
                kinectCoreWindow = null;
                reader = null;
                colorFrameDescription = null;
                toneTriggerList = null;

                disposed = true;
                isLoaded = false;
            }
        }

        #endregion

        private void StartPlaying_Loaded (object sender, RoutedEventArgs e)
        {
            Grid_Main.Opacity = 0;
            KinectRegion.SetKinectRegion(this, region);
            kinectCoreWindow = KinectCoreWindow.GetForCurrentThread();
            region.KinectSensor = KinectSensor.GetDefault();
            region.Loaded += (se, ev) => region.SetKinectTwoPersonSystemEngagement();
            isEnsembleMode = false;
            playing = false;
            outDeviceID = 0;
            trackingBodyIndex = 0;
            sensor = region.KinectSensor;
            sensor.Open();

            reader = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Body);
            reader.MultiSourceFrameArrived += reader_MultiSourceFrameArrived;
            colorFrameDescription = sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
            dataList = new List<PianoControlData>();
            trackingData = new PianoControlData[2];
            toneTriggerList = new List<ToneTriggerHandler>();
            //kinectCoreWindow.PointerMoved += kinectCoreWindow_PointerMoved;

            this.drawingGroup = new DrawingGroup();
            this.imageSource = new DrawingImage(this.drawingGroup);

            if (outDevice == null)
            {
                outDevice = new OutputDevice(outDeviceID);
            }

            for (int i = 0; i < sensor.BodyFrameSource.BodyCount; ++i)
            {
                dataList.Add(new PianoControlData(i));
                toneTriggerList.Add(new ToneTriggerHandler());
            }

            isLoaded = true;
            StoryboardHandler.InitHitStoryBoard(this, "EnterStoryboard");
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
                        colorBitmap.Lock();
                        if (( colorFrameDescription.Width == this.colorBitmap.PixelWidth ) && ( colorFrameDescription.Height == colorBitmap.PixelHeight ))
                        {
                            colorFrame.CopyConvertedFrameDataToIntPtr(this.colorBitmap.BackBuffer, (uint)( colorFrameDescription.Width * colorFrameDescription.Height * 4 ), ColorImageFormat.Bgra);
                            colorBitmap.AddDirtyRect(new Int32Rect(0, 0, colorBitmap.PixelWidth, colorBitmap.PixelHeight));
                        }
                        colorBitmap.Unlock();
                        test.Source = colorBitmap;
                    }
                }
            }

            Boolean dataReceived = false;

            using (BodyFrame frame = reference.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (bodies == null)
                    {
                        bodies = new Body[frame.BodyCount];
                    }
                    frame.GetAndRefreshBodyData(bodies);
                    dataReceived = true;
                }
            }

            if (dataReceived)
            {
                if (bodies != null)
                {
                    using (DrawingContext dc = this.drawingGroup.Open())
                    {
                        foreach (var item in bodies.Select((value, i) => new { i, value }))
                        {
                            if (item.value.IsTracked)
                            {
                                /*IReadOnlyDictionary<JointType, Joint> joints = item.value.Joints;
                                Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();
                                foreach (JointType jointType in joints.Keys)
                                {
                                    ColorSpacePoint colorSpacePoint = sensor.CoordinateMapper.MapCameraPointToColorSpace(joints[jointType].Position);
                                    jointPoints[jointType] = new Point(colorSpacePoint.X, colorSpacePoint.Y);
                                }
                                Extensions.DrawBody(joints, jointPoints, dc);*/
                                toneTriggerList[item.i].UpdateBodyData(item.value);

                                var a = PianoControl.cnvPiano.Children[12] as PianoKeyWPF;
                                var x = item.value.Joints[JointType.ShoulderLeft].Position.X * Canvas_Control.ActualWidth + Canvas_Control.ActualWidth / 2;
                                var y = item.value.Joints[JointType.ShoulderLeft].Position.Y * Canvas_Control.ActualHeight + Canvas_Control.ActualHeight / 2;
                                a.Margin = new Thickness(0, 0, 0, 0);
                                Canvas.SetLeft(a, x);
                                Canvas.SetTop(a, y);
                                X.Content = x.ToString();
                                Y.Content = y.ToString();
                                

                                if (toneTriggerList[item.i].CheckToneDoTrigger())
                                    ( (PianoKeyWPF)PianoControl.cnvPiano.Children[12] ).PressPianoKey();
                                if (toneTriggerList[item.i].CheckToneReTrigger())
                                    ( (PianoKeyWPF)PianoControl.cnvPiano.Children[14] ).PressPianoKey();
                                if (toneTriggerList[item.i].CheckToneMiTrigger())
                                    ( (PianoKeyWPF)PianoControl.cnvPiano.Children[16] ).PressPianoKey();
                                if (toneTriggerList[item.i].CheckToneFaTrigger())
                                    ( (PianoKeyWPF)PianoControl.cnvPiano.Children[17] ).PressPianoKey();
                                if (toneTriggerList[item.i].CheckToneSoTrigger())
                                    ( (PianoKeyWPF)PianoControl.cnvPiano.Children[19] ).PressPianoKey();
                                /*if (toneTriggerList[item.i].CheckToneLaTrigger())
                                    ( (PianoKeyWPF)PianoControl.cnvPiano.Children[21] ).PressPianoKey();*/
                            }
                            dataList[item.i].UpdateBodyData(item.value);
                        }
                        foreach (var item in dataList)
                        {
                            if (item.TrackingId != 0 && trackingBodyIndex < 2)
                            {
                                trackingData[trackingBodyIndex] = item;
                                trackingBodyIndex++;
                            }
                        }
                        this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, colorFrameDescription.Width, colorFrameDescription.Height));
                    }
                }
            }
            test.Source = this.imageSource;
        }
        
        private void kinectCoreWindow_PointerMoved (object sender, KinectPointerEventArgs args)
        {
            KinectPointerPoint point = args.CurrentPoint;
            if (lastTime == TimeSpan.Zero || lastTime != point.Properties.BodyTimeCounter)
            {
                lastTime = point.Properties.BodyTimeCounter;
                PianoControl.mainScreen.Children.Clear();
                PianoControl1.mainScreen.Children.Clear();
            }

            foreach (var item in dataList)
            {
                item.GetPointerPosition(point);
            }

            if (trackingData[0] != null && point.Properties.BodyTrackingId == trackingData[0].TrackingId)
            {
                Point screenRelative = new Point(
                point.Position.X * PianoControl.mainScreen.ActualWidth,
                point.Position.Y * PianoControl.mainScreen.ActualHeight);
                RenderPointer(point.Position, PianoControl, trackingData[0].TrackingId, point.Properties.HandType);
                foreach (PianoKeyWPF relativeTo in PianoControl.cnvPiano.Children)
                {
                    Point relativeToElement = PianoControl.mainScreen.TranslatePoint(screenRelative, relativeTo);
                    Boolean insideElement = (
                        relativeToElement.X >= 0 &&
                        relativeToElement.X < relativeTo.ActualWidth &&
                        relativeToElement.Y >= 0 &&
                        relativeToElement.Y < relativeTo.ActualHeight );
                    if (point.Properties.HandType == HandType.RIGHT && insideElement)
                    {
                        if (trackingData[0].IsRightHandLasso && !relativeTo.IsPianoKeyPressed)
                        {
                            relativeTo.PressPianoKey();
                        }
                        else if (!trackingData[0].IsRightHandLasso && relativeTo.IsPianoKeyPressed)
                        {
                            relativeTo.ReleasePianoKey();
                        }
                    }
                    else if (point.Properties.HandType == HandType.LEFT && insideElement)
                    {
                        if (trackingData[0].IsLeftHandLasso && !relativeTo.IsPianoKeyPressed)
                        {
                            relativeTo.PressPianoKey();
                        }
                        else if (!trackingData[0].IsLeftHandLasso && relativeTo.IsPianoKeyPressed)
                        {
                            relativeTo.ReleasePianoKey();
                        }
                    }
                }
            }
            else if (trackingData[1] != null && point.Properties.BodyTrackingId == trackingData[1].TrackingId)
            {
                Point screenRelative = new Point(
                point.Position.X * PianoControl1.mainScreen.ActualWidth,
                point.Position.Y * PianoControl1.mainScreen.ActualHeight);
                RenderPointer(point.Position, PianoControl1, trackingData[1].TrackingId, point.Properties.HandType);
                foreach (PianoKeyWPF relativeTo in PianoControl1.cnvPiano.Children)
                {
                    Point relativeToElement = PianoControl1.mainScreen.TranslatePoint(screenRelative, relativeTo);
                    Boolean insideElement = (
                        relativeToElement.X >= 0 &&
                        relativeToElement.X < relativeTo.ActualWidth &&
                        relativeToElement.Y >= 0 &&
                        relativeToElement.Y < relativeTo.ActualHeight );
                    if (point.Properties.HandType == HandType.RIGHT && insideElement)
                    {
                        if (trackingData[1].IsRightHandLasso && !relativeTo.IsPianoKeyPressed)
                        {
                            relativeTo.PressPianoKey();
                        }
                        else if (!trackingData[1].IsRightHandLasso && relativeTo.IsPianoKeyPressed)
                        {
                            relativeTo.ReleasePianoKey();
                        }
                    }
                    else if (point.Properties.HandType == HandType.LEFT && insideElement)
                    {
                        if (trackingData[1].IsLeftHandLasso && !relativeTo.IsPianoKeyPressed)
                        {
                            relativeTo.PressPianoKey();
                        }
                        else if (!trackingData[1].IsLeftHandLasso && relativeTo.IsPianoKeyPressed)
                        {
                            relativeTo.ReleasePianoKey();
                        }
                    }
                }
            }
        }

        private void RenderPointer (PointF position, PianoControlWPF control, UInt64 trackingId, HandType handType)
        {
            StackPanel cursor = null;
            if (cursor == null)
            {
                cursor = new StackPanel();
                control.mainScreen.Children.Add(cursor);
            }

            cursor.Children.Clear();

            StackPanel sp = new StackPanel()
            {
                Margin = new Thickness(-5, -5, 0, 0),
                Orientation = Orientation.Horizontal
            };

            Ellipse cursorEllipse = new Ellipse()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Height = dotHeight,
                Width = dotWidth,
                Margin = new Thickness(5)
            };

            sp.Children.Add(cursorEllipse);
            cursor.Children.Add(sp);
            cursor.Children.Add(new TextBlock() { Text = "BodyTrackingId: " + trackingId });
            cursor.Children.Add(new TextBlock() { Text = "HandType: " + handType });
            
            if (control == PianoControl)
                cursorEllipse.Fill = ( handType == HandType.LEFT) ? Brushes.Green : Brushes.Yellow;
            else
                cursorEllipse.Fill = ( handType == HandType.LEFT ) ? Brushes.Red : Brushes.Blue;
            Canvas.SetLeft(cursor, position.X * control.mainScreen.ActualWidth - dotWidth / 2);
            Canvas.SetTop(cursor, position.Y * control.mainScreen.ActualHeight - dotHeight / 2);
        }

        /*private void performLassoClick (Point relative, UIElement relativeTo)
        {
            var clickedButton = relativeTo as Button;
            if (clickedButton != null)
            {
                Point relativeToElement = region.TranslatePoint(relative, clickedButton);
                bool insideElement = ( relativeToElement.X >= 0 && relativeToElement.X < clickedButton.ActualWidth
                    && relativeToElement.Y >= 0 && relativeToElement.Y < clickedButton.ActualHeight );
                if (insideElement && isRightLasso && clickedButton.IsHitTestVisible)
                {
                    clickedButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    VisualStateManager.GoToState(clickedButton, "Pressed", true);
                }
            }
        }*/

        /*private void performLassoPianoClick (Point screenRelative, UIElement relativeTo, HandType handType, PianoControlWPF control, PianoControlData item)
        {
            Point relativeToElement = PianoControl.mainScreen.TranslatePoint(screenRelative, relativeTo);
            Boolean insideElement = (
                relativeToElement.X >= 0 &&
                relativeToElement.X < relativeTo.ActualWidth &&
                relativeToElement.Y >= 0 &&
                relativeToElement.Y < relativeTo.ActualHeight );
            if (handType == HandType.RIGHT && insideElement)
            {
                if (item.IsRightHandLasso && !relativeTo.IsPianoKeyPressed)
                {
                    relativeTo.PressPianoKey();
                }
                else if (!item.IsRightHandLasso && relativeTo.IsPianoKeyPressed)
                {
                    relativeTo.ReleasePianoKey();
                }
            }
            else if (handType == HandType.LEFT && insideElement)
            {
                if (item.IsLeftHandLasso && !relativeTo.IsPianoKeyPressed)
                {
                    relativeTo.PressPianoKey();
                }
                else if (!item.IsLeftHandLasso && relativeTo.IsPianoKeyPressed)
                {
                    relativeTo.ReleasePianoKey();
                }
            }


            var clickedButton = relativeTo as PianoKeyWPF;
            if (clickedButton != null)
            {
                Point relativeToElement = control.mainScreen.TranslatePoint(relative, clickedButton);
                bool insideElement = ( relativeToElement.X >= 0 && relativeToElement.X < clickedButton.ActualWidth
                    && relativeToElement.Y >= 0 && relativeToElement.Y < clickedButton.ActualHeight );
                if (control == PianoControl)
                {
                    if (handType == HandType.RIGHT && insideElement)
                    {
                        if (item.IsRightHandLasso && !clickedButton.IsPianoKeyPressed)
                        {
                            clickedButton.PressPianoKey();
                        }
                        else if (!item.IsRightHandLasso && clickedButton.IsPianoKeyPressed)
                        {
                            clickedButton.ReleasePianoKey();
                        }
                    }
                    else if (handType == HandType.LEFT)
                    {
                        if (insideElement && isLeftLasso && !clickedButton.IsPianoKeyPressed)
                        {
                            clickedButton.PressPianoKey();
                        }
                        else if (insideElement && isLeftClosed && clickedButton.IsPianoKeyPressed)
                        {
                            clickedButton.ReleasePianoKey();
                        }
                    }
                }
                else if (control == PianoControl1)
                {
                    if (handType == HandType.RIGHT)
                    {
                        if (insideElement && isRightLasso1 && clickedButton.IsHitTestVisible && !clickedButton.IsPianoKeyPressed)
                        {
                            clickedButton.PressPianoKey();
                        }
                        else if (insideElement && isRightClosed1 && clickedButton.IsHitTestVisible && clickedButton.IsPianoKeyPressed)
                        {
                            clickedButton.ReleasePianoKey();
                        }
                    }
                    else if (handType == HandType.LEFT)
                    {
                        if (insideElement && isLeftLasso1 && !clickedButton.IsPianoKeyPressed)
                        {
                            clickedButton.PressPianoKey();
                        }
                        else if (insideElement && isLeftClosed1 && clickedButton.IsPianoKeyPressed)
                        {
                            clickedButton.ReleasePianoKey();
                        }
                    }
                }
            }
        }*/

        private void Button_Click (object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            switch (button.Name)
            {
                case "Button_Ensemble":
                    if (isEnsembleMode)
                    {
                        setEnsembleButtonMode(true);
                        StoryboardHandler.InitHitStoryBoard(this, "SingleStoryboard");

                    }
                    else
                    {
                        setEnsembleButtonMode(false);
                        StoryboardHandler.InitHitStoryBoard(this, "EnsembleStoryboard");
                    }
                    break;
                case "Button_Back":
                    setEnsembleButtonMode(true);
                    Dispose(true);
                    StoryboardHandler.InitHitStoryBoard(this, "ExitStoryboard", () => Switcher.Switch("MainMenu"));
                    break;
                default:
                    break;
            }
        }

        private void setEnsembleButtonMode (Boolean isEnsembleMode)
        {
            if (!isEnsembleMode)
            {
                Button_Ensemble.Content = "獨奏模式";
                Button_Ensemble.Background = new ImageBrush() { ImageSource = ( (BitmapImage)this.Resources["Image_Single"] ) };
            }
            else
            {
                Button_Ensemble.Content = "合奏模式";
                Button_Ensemble.Background = new ImageBrush() { ImageSource = ( (BitmapImage)this.Resources["Image_Ensemble"] ) };
            }
            this.isEnsembleMode = !isEnsembleMode;
        }

        private void PianoControl_PianoKeyDown (object sender, PianoKeyEventArgs e)
        {
            #region Guard

            if (playing)
            {
                return;
            }

            #endregion
            if (!disposed)
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
            if (!disposed)
                outDevice.Send(new ChannelMessage(ChannelCommand.NoteOff, 0, e.NoteID, 0));
        }
    }
}