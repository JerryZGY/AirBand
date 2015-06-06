﻿using System;
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

namespace KinectAirBand.Pages
{
    /// <summary>
    /// Interaction logic for StartPlaying.xaml
    /// </summary>
    public partial class Playing : UserControl, IDisposable
    {
        private Boolean disposed = false;
        private Boolean isLoaded = false;
        private Body[] bodies;
        private TimeSpan lastTime;
        private KinectSensor sensor;
        private OutputDevice outDevice;
        private UInt32 bitmapBackBufferSize;
        private List<BodyViewModel> bodyList;
        private WriteableBitmap bitmap = null;
        private MultiSourceFrameReader reader;
        private FrameDescription colorFrameDesc;
        private KinectCoreWindow kinectCoreWindow;
        private DepthSpacePoint[] colorMappedToDepthPoints = new DepthSpacePoint[1920 * 1080];

        public Playing (EnvironmentVariablesViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }

        #region IDisposable

        ~Playing ()
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

            if (isLoaded)
            {
                if (disposing && isLoaded)
                {
                    outDevice.Close();
                    outDevice.Dispose();
                    reader.Dispose();
                }

                kinectCoreWindow.PointerMoved -= kinectCoreWindow_PointerMoved;
                sensor = null;
                bodies = null;
                kinectCoreWindow = null;
                bitmapBackBufferSize = 0;
                //bodyList = null;
                reader = null;
                colorFrameDesc = null;
                bitmap = null;
                Image_BackgroundRemoval.Source = null;
                disposed = true;
                isLoaded = false;
            }
        }

        #endregion

        private void Playing_Loaded (object sender, RoutedEventArgs e)
        {
            Grid_Main.Opacity = 0;
            kinectCoreWindow = KinectCoreWindow.GetForCurrentThread();
            sensor = KinectSensor.GetDefault();
            sensor.Open();
            reader = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Body | FrameSourceTypes.Depth | FrameSourceTypes.BodyIndex);
            colorFrameDesc = sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            bodyList = new List<BodyViewModel>();

            if (outDevice == null)
            {
                outDevice = new OutputDevice(0);
            }

            for (int i = 0; i < sensor.BodyFrameSource.BodyCount; ++i)
            {
                bodyList.Add(new BodyViewModel(i));
            }

            StoryboardHandler.InitHitStoryBoard(this, "EnterStoryboard", () =>
            {
                if (!isLoaded)
                {
                    reader.MultiSourceFrameArrived += reader_MultiSourceFrameArrived;
                    bitmap = new WriteableBitmap(1920, 1080, 96.0, 96.0, PixelFormats.Bgra32, null);
                    bitmapBackBufferSize = (uint)( ( bitmap.BackBufferStride * ( bitmap.PixelHeight - 1 ) ) + ( bitmap.PixelWidth * 4 ) );
                    Image_BackgroundRemoval.Source = bitmap;
                    kinectCoreWindow.PointerMoved += kinectCoreWindow_PointerMoved;
                }
                isLoaded = true;
            });

            disposed = false;
        }

        private void reader_MultiSourceFrameArrived (object sender, MultiSourceFrameArrivedEventArgs e)
        {
            MultiSourceFrame multiSourceFrame = e.FrameReference.AcquireFrame();

            using (var frame = multiSourceFrame.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    Canvas_Test.Children.Clear();
                    bodies = new Body[frame.BodyFrameSource.BodyCount];

                    frame.GetAndRefreshBodyData(bodies);

                    foreach (var item in bodies.Select((value, i) => new {i ,value}))
                    {
                        BodyViewModel body = bodyList[item.i];
                        body.SetBodyData(item.value);
                        if (item.value.IsTracked)
                        {
                            //2維中心點
                            Point centerPoint = coordinateMap(item.value.Joints[JointType.SpineMid].Position);
                            //2維下心點
                            Point spinePoint = coordinateMap(item.value.Joints[JointType.SpineBase].Position);
                            //2維定位點
                            Point locatePoint = coordinateMap(item.value.Joints[JointType.Head].Position);
                            //2維左異點
                            Point leftVariabPoint = coordinateMap(item.value.Joints[JointType.HandLeft].Position);
                            //2維右異點
                            Point rightVariabPoint = coordinateMap(item.value.Joints[JointType.HandRight].Position);
                            //2維中肩點
                            Point shouldPoint = coordinateMap(item.value.Joints[JointType.SpineShoulder].Position);
                            //左手狀態
                            HandState leftHandState = item.value.HandLeftState;
                            //右手狀態
                            HandState rightHandState = item.value.HandRightState;
                            //更新體感數據
                            body.UpdateBodyData(centerPoint, spinePoint, locatePoint, leftVariabPoint, rightVariabPoint, shouldPoint, leftHandState, rightHandState);
                            if (body.Instrument != null)
                                body.UpdateInstrument();

                            /*Grid_GuitarControls.Children.Add(
                                new Line() { Stroke = Brushes.Green, X1 = spinePoint.X, Y1 = (spinePoint.Y + centerPoint.Y) / 2, X2 = leftVariabPoint.X, Y2 = leftVariabPoint.Y, StrokeThickness = 20 });*/
                        }
                        else
                        {
                            if (body.Instrument != null)
                            {
                                body.ClearInstrument(Grid_PianoControls);
                            }
                        }
                    }
                }
            }

            DepthFrame depthFrame = null;
            ColorFrame colorFrame = null;
            BodyIndexFrame bodyIndexFrame = null;
            bool isBitmapLocked = false;
            if (multiSourceFrame == null)
            {
                return;
            }
            try
            {
                depthFrame = multiSourceFrame.DepthFrameReference.AcquireFrame();
                colorFrame = multiSourceFrame.ColorFrameReference.AcquireFrame();
                bodyIndexFrame = multiSourceFrame.BodyIndexFrameReference.AcquireFrame();
                if (( depthFrame == null ) || ( colorFrame == null ) || ( bodyIndexFrame == null ))
                {
                    return;
                }
                FrameDescription depthFrameDescription = depthFrame.FrameDescription;
                Int32 depthWidth = depthFrameDescription.Width;
                Int32 depthHeight = depthFrameDescription.Height;
                using (KinectBuffer depthFrameData = depthFrame.LockImageBuffer())
                {
                    sensor.CoordinateMapper.MapColorFrameToDepthSpaceUsingIntPtr(
                        depthFrameData.UnderlyingBuffer,
                        depthFrameData.Size,
                        this.colorMappedToDepthPoints);
                }
                depthFrame.Dispose();
                depthFrame = null;
                this.bitmap.Lock();
                isBitmapLocked = true;
                colorFrame.CopyConvertedFrameDataToIntPtr(this.bitmap.BackBuffer, this.bitmapBackBufferSize, ColorImageFormat.Bgra);
                colorFrame.Dispose();
                colorFrame = null;
                using (KinectBuffer bodyIndexData = bodyIndexFrame.LockImageBuffer())
                {
                    unsafe
                    {
                        byte* bodyIndexDataPointer = (byte*)bodyIndexData.UnderlyingBuffer;
                        int colorMappedToDepthPointCount = this.colorMappedToDepthPoints.Length;
                        fixed (DepthSpacePoint* colorMappedToDepthPointsPointer = this.colorMappedToDepthPoints)
                        {
                            uint* bitmapPixelsPointer = (uint*)this.bitmap.BackBuffer;
                            for (int colorIndex = 0; colorIndex < colorMappedToDepthPointCount; ++colorIndex)
                            {
                                float colorMappedToDepthX = colorMappedToDepthPointsPointer[colorIndex].X;
                                float colorMappedToDepthY = colorMappedToDepthPointsPointer[colorIndex].Y;
                                if (!float.IsNegativeInfinity(colorMappedToDepthX) &&
                                    !float.IsNegativeInfinity(colorMappedToDepthY))
                                {
                                    int depthX = (int)( colorMappedToDepthX + 0.5f );
                                    int depthY = (int)( colorMappedToDepthY + 0.5f );
                                    if (( depthX >= 0 ) && ( depthX < depthWidth ) && ( depthY >= 0 ) && ( depthY < depthHeight ))
                                    {
                                        int depthIndex = ( depthY * depthWidth ) + depthX;
                                        if (bodyIndexDataPointer[depthIndex] != 0xff)
                                        {
                                            continue;
                                        }
                                    }
                                }
                                bitmapPixelsPointer[colorIndex] = 0;
                            }
                        }
                        this.bitmap.AddDirtyRect(new Int32Rect(0, 0, this.bitmap.PixelWidth, this.bitmap.PixelHeight));
                    }
                }
            }
            finally
            {
                if (isBitmapLocked)
                {
                    this.bitmap.Unlock();
                }
                if (depthFrame != null)
                {
                    depthFrame.Dispose();
                }
                if (colorFrame != null)
                {
                    colorFrame.Dispose();
                }
                if (bodyIndexFrame != null)
                {
                    bodyIndexFrame.Dispose();
                }
            }
        }

        private Point coordinateMap (CameraSpacePoint position)
        {
            var point = sensor.CoordinateMapper.MapCameraPointToColorSpace(position);
            return new Point(
                float.IsInfinity(point.X) ? 0 : point.X / colorFrameDesc.Width * Canvas_Pointer.ActualWidth,
                float.IsInfinity(point.Y) ? 0 : point.Y / colorFrameDesc.Height * Canvas_Pointer.ActualHeight);
        }

        private void kinectCoreWindow_PointerMoved (object sender, KinectPointerEventArgs args)
        {
            KinectPointerPoint point = args.CurrentPoint;
            UInt64 trackingId = point.Properties.BodyTrackingId;
            if (lastTime == TimeSpan.Zero || lastTime != point.Properties.BodyTimeCounter)
            {
                lastTime = point.Properties.BodyTimeCounter;
            }

            Point screenRelative = new Point(
            point.Position.X * Canvas_Pointer.ActualWidth,
            point.Position.Y * Canvas_Pointer.ActualHeight);
            RenderPointer(point.Position, Canvas_Pointer, point.Properties.HandType);
            if (point.Properties.IsEngaged)
            {
                performLassoClick(screenRelative, trackingId, Button_Piano);
                performLassoClick(screenRelative, trackingId, Button_Guitar);
                performLassoClick(screenRelative, trackingId, Button_Clear);
            }
        }

        private void RenderPointer (PointF position, Canvas canvas, HandType handType)
        {
            Canvas.SetLeft(Pointer, position.X * canvas.ActualWidth);
            Canvas.SetTop(Pointer, position.Y * canvas.ActualHeight);
            /*StackPanel cursor = null;
            if (cursor == null)
            {
                cursor = new StackPanel();
                canvas.Children.Add(cursor);
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
            cursor.Children.Add(new TextBlock() { Text = "HandType: " + handType });

            cursorEllipse.Fill = ( handType == HandType.LEFT ) ? Brushes.Green : Brushes.Yellow;
            Canvas.SetLeft(cursor, position.X * canvas.ActualWidth - dotWidth / 2);
            Canvas.SetTop(cursor, position.Y * canvas.ActualHeight - dotHeight / 2);*/
        }

        private void performLassoClick (Point relative, UInt64 trackingId, UIElement relativeTo)
        {
            Button clickedButton = relativeTo as Button;
            if (clickedButton != null)
            {
                Point relativeToElement = Canvas_Pointer.TranslatePoint(relative, clickedButton);
                Boolean insideElement = ( relativeToElement.X >= 0 && relativeToElement.X < clickedButton.ActualWidth
                    && relativeToElement.Y >= 0 && relativeToElement.Y < clickedButton.ActualHeight );
                Boolean isLasso = bodyList.Where(x => x.TrackingId == trackingId).First().RightHandState == HandState.Lasso;
                if (insideElement && clickedButton.IsHitTestVisible)
                {
                    VisualStateManager.GoToState(clickedButton, "MouseOver", false);
                }
                if (insideElement && isLasso && clickedButton.IsHitTestVisible)
                {
                    clickedButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, trackingId));
                    VisualStateManager.GoToState(clickedButton, "Pressed", true);
                }
                else if (!insideElement && !isLasso && clickedButton.IsHitTestVisible)
                {
                    VisualStateManager.GoToState(clickedButton, "Normal", true);
                }
            }
        }

        private void Button_Click (object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            BodyViewModel body = null;
            if (e.OriginalSource.GetType() == typeof(UInt64))
            {
                body = bodyList.Where(x => x.TrackingId == Convert.ToUInt64(e.OriginalSource)).First();
            }

            switch (button.Name)
            {
                case "Button_Piano":
                    if (body != null && (body.Instrument == null || body.Instrument.GetType() != typeof(PianoControl)))
                    {
                        body.ClearInstrument(Grid_GuitarControls);
                        body.SetInstrument(new PianoControl(outDevice, body));
                        Grid_PianoControls.Children.Add(body.Instrument);
                    }
                    break;
                case "Button_Guitar":
                    if (body != null && ( body.Instrument == null || body.Instrument.GetType() != typeof(GuitarControl) ))
                    {
                        body.ClearInstrument(Grid_PianoControls);
                        body.SetInstrument(new GuitarControl(outDevice, body));
                        Grid_GuitarControls.Children.Add(body.Instrument);
                    }
                    break;
                case "Button_Clear":
                    if (body != null && body.Instrument != null)
                    {
                        body.ClearInstrument(Grid_PianoControls);
                    }
                    break;
                case "Button_Back":
                    Dispose(true);
                    StoryboardHandler.InitHitStoryBoard(this, "ExitStoryboard", () => Switcher.Switch("MainMenu"));
                    break;
                default:
                    break;
            }
        }
    }
}