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
                return bitmap;
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
        private PianoControlData[] trackingData;
        private List<ToneTriggerHandler> toneTriggerList;
        private DrawingGroup drawingGroup;
        private DrawingImage imageSource;
        private WriteableBitmap bitmap = null;

        private double centerXPoint = 0;
        private double centerYPoint = 0;
        Rectangle[] pianoKeys = new Rectangle[8];
        DepthSpacePoint[] colorMappedToDepthPoints = new DepthSpacePoint[1920 * 1080];
        private uint bitmapBackBufferSize;
        private Body trackingBody = null;

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
            sensor = region.KinectSensor;
            sensor.Open();

            reader = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Body | FrameSourceTypes.Depth | FrameSourceTypes.BodyIndex);
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

            pianoKeys = pianoKeys.Select(x =>
            {
                x = new Rectangle()
                {
                    Fill = Brushes.White,
                    Width = 48.78,
                    Height = 255,
                    Opacity = 0.8,
                    
                };
                return x;
            }).ToArray();

            this.bitmap = new WriteableBitmap(1920, 1080, 96.0, 96.0, PixelFormats.Bgra32, null);
            // Calculate the WriteableBitmap back buffer size
            this.bitmapBackBufferSize = (uint)( ( this.bitmap.BackBufferStride * ( this.bitmap.PixelHeight - 1 ) ) + ( this.bitmap.PixelWidth * 4) );
        }

        private void reader_MultiSourceFrameArrived (object sender, MultiSourceFrameArrivedEventArgs e)
        {
            MultiSourceFrame multiSourceFrame = e.FrameReference.AcquireFrame();

            using (var frame = multiSourceFrame.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    canvas.Children.Clear();
                    pointCanvas.Children.Clear();

                    bodies = new Body[frame.BodyFrameSource.BodyCount];

                    frame.GetAndRefreshBodyData(bodies);

                    foreach (var body in bodies)
                    {
                        if (body.IsTracked)
                        {
                            trackingBody = body;
                            //(head-150) (head-100) (head-50) (head) (head+50) (head+100) (head+150)
                            //   leftThree, leftTwo, leftOne, center, rightOne, rightTwo, rightThree
                            // COORDINATE MAPPING
                            Joint joint = body.Joints[JointType.Head];
                            Joint hand = body.Joints[JointType.HandRight];

                            // 3D space point
                            ColorSpacePoint colorPoint = sensor.CoordinateMapper.MapCameraPointToColorSpace(joint.Position);
                            ColorSpacePoint handColorPoint = sensor.CoordinateMapper.MapCameraPointToColorSpace(hand.Position);
                            // 2D space point
                            centerXPoint = float.IsInfinity(colorPoint.X) ? 0 : colorPoint.X / 1920 * canvas.ActualWidth - 48.75 / 2;
                            centerYPoint = float.IsInfinity(colorPoint.Y) ? 0 : colorPoint.Y / 1080 * canvas.ActualHeight - 255 / 2;

                            Point handPoint = new Point();
                            handPoint.X = float.IsInfinity(handColorPoint.X) ? 0 : handColorPoint.X / 1920 * canvas.ActualWidth - 25;
                            handPoint.Y = float.IsInfinity(handColorPoint.Y) ? 0 : handColorPoint.Y / 1080 * canvas.ActualHeight - 25;
                                
                            var rightHand = new Ellipse() { Fill = Brushes.Red, Width = 50, Height = 50 };
                            Canvas.SetLeft(rightHand, handPoint.X);
                            Canvas.SetTop(rightHand, handPoint.Y);
                            pointCanvas.Children.Add(rightHand);
                            Rect rightHandRect = new Rect(handPoint.X, handPoint.Y, rightHand.ActualWidth, rightHand.ActualHeight);

                            for (int i = 0; i < pianoKeys.Length; i++)
                            {
                                switch (i)
                                {
                                    case 0:
                                        var x = centerXPoint - 50;
                                        var y = centerYPoint + 200;
                                        pianoKeys[i].RenderTransformOrigin = new Point(0, 1);
                                        pianoKeys[i].RenderTransform = new RotateTransform() { Angle = -80 };
                                        Canvas.SetLeft(pianoKeys[i], x);
                                        Canvas.SetTop(pianoKeys[i], y);
                                        HitTestResult result = VisualTreeHelper.HitTest(pianoKeys[i], handPoint);
                                        Rect keyRect = new Rect(x, y, pianoKeys[i].ActualWidth, pianoKeys[i].ActualHeight);
                                        //if (rightHandRect.IntersectsWith(keyRect))
                                            //( (PianoKeyWPF)PianoControl.cnvPiano.Children[12] ).PressPianoKey();
                                        break;
                                    case 1:
                                        pianoKeys[i].RenderTransformOrigin = new Point(0, 1);
                                        pianoKeys[i].RenderTransform = new RotateTransform() { Angle = -60 };
                                        Canvas.SetLeft(pianoKeys[i], centerXPoint - 80);
                                        Canvas.SetTop(pianoKeys[i], centerYPoint + 150);
                                        //( (PianoKeyWPF)PianoControl.cnvPiano.Children[14] ).PressPianoKey();
                                        break;
                                    case 2:
                                        pianoKeys[i].RenderTransformOrigin = new Point(0, 1);
                                        pianoKeys[i].RenderTransform = new RotateTransform() { Angle = -40 };
                                        Canvas.SetLeft(pianoKeys[i], centerXPoint - 80);
                                        Canvas.SetTop(pianoKeys[i], centerYPoint + 100);
                                        //( (PianoKeyWPF)PianoControl.cnvPiano.Children[16] ).PressPianoKey();
                                        break;
                                    case 3:
                                        pianoKeys[i].RenderTransformOrigin = new Point(0, 1);
                                        pianoKeys[i].RenderTransform = new RotateTransform() { Angle = -30 };
                                        Canvas.SetLeft(pianoKeys[i], centerXPoint - 50);
                                        Canvas.SetTop(pianoKeys[i], centerYPoint + 50);
                                        //( (PianoKeyWPF)PianoControl.cnvPiano.Children[17] ).PressPianoKey();
                                        break;
                                    case 4:
                                        pianoKeys[i].RenderTransformOrigin = new Point(1, 1);
                                        pianoKeys[i].RenderTransform = new RotateTransform() { Angle = 30 };
                                        Canvas.SetLeft(pianoKeys[i], centerXPoint + 50);
                                        Canvas.SetTop(pianoKeys[i], centerYPoint + 50);
                                        //( (PianoKeyWPF)PianoControl.cnvPiano.Children[19] ).PressPianoKey();
                                        break;
                                    case 5:
                                        pianoKeys[i].RenderTransformOrigin = new Point(1, 1);
                                        pianoKeys[i].RenderTransform = new RotateTransform() { Angle = 40 };
                                        Canvas.SetLeft(pianoKeys[i], centerXPoint + 80);
                                        Canvas.SetTop(pianoKeys[i], centerYPoint + 100);
                                        //( (PianoKeyWPF)PianoControl.cnvPiano.Children[21] ).PressPianoKey();
                                        break;
                                    case 6:
                                        pianoKeys[i].RenderTransformOrigin = new Point(1, 1);
                                        pianoKeys[i].RenderTransform = new RotateTransform() { Angle = 60 };
                                        Canvas.SetLeft(pianoKeys[i], centerXPoint + 80);
                                        Canvas.SetTop(pianoKeys[i], centerYPoint + 150);
                                        break;
                                    case 7:
                                        pianoKeys[i].RenderTransformOrigin = new Point(1, 1);
                                        pianoKeys[i].RenderTransform = new RotateTransform() { Angle = 80 };
                                        Canvas.SetLeft(pianoKeys[i], centerXPoint + 50);
                                        Canvas.SetTop(pianoKeys[i], centerYPoint + 200);
                                        break;
                                    default:
                                        break;
                                }
                                canvas.Children.Add(pianoKeys[i]);
                            }
                        }
                    }
                }
            }


            int depthWidth = 0;
            int depthHeight = 0;

            DepthFrame depthFrame = null;
            ColorFrame colorFrame = null;
            BodyIndexFrame bodyIndexFrame = null;
            bool isBitmapLocked = false;
            // If the Frame has expired by the time we process this event, return.
            if (multiSourceFrame == null)
            {
                return;
            }
            // We use a try/finally to ensure that we clean up before we exit the function.  
            // This includes calling Dispose on any Frame objects that we may have and unlocking the bitmap back buffer.
            try
            {
                depthFrame = multiSourceFrame.DepthFrameReference.AcquireFrame();
                colorFrame = multiSourceFrame.ColorFrameReference.AcquireFrame();
                bodyIndexFrame = multiSourceFrame.BodyIndexFrameReference.AcquireFrame();
                // If any frame has expired by the time we process this event, return.
                // The "finally" statement will Dispose any that are not null.
                if (( depthFrame == null ) || ( colorFrame == null ) || ( bodyIndexFrame == null ))
                {
                    return;
                }
                // Process Depth
                FrameDescription depthFrameDescription = depthFrame.FrameDescription;
                depthWidth = depthFrameDescription.Width;
                depthHeight = depthFrameDescription.Height;
                // Access the depth frame data directly via LockImageBuffer to avoid making a copy
                using (KinectBuffer depthFrameData = depthFrame.LockImageBuffer())
                {
                    sensor.CoordinateMapper.MapColorFrameToDepthSpaceUsingIntPtr(
                        depthFrameData.UnderlyingBuffer,
                        depthFrameData.Size,
                        this.colorMappedToDepthPoints);
                }
                // We're done with the DepthFrame 
                depthFrame.Dispose();
                depthFrame = null;
                // Process Color
                // Lock the bitmap for writing
                this.bitmap.Lock();
                isBitmapLocked = true;
                colorFrame.CopyConvertedFrameDataToIntPtr(this.bitmap.BackBuffer, this.bitmapBackBufferSize, ColorImageFormat.Bgra);
                // We're done with the ColorFrame 
                colorFrame.Dispose();
                colorFrame = null;
                // We'll access the body index data directly to avoid a copy
                using (KinectBuffer bodyIndexData = bodyIndexFrame.LockImageBuffer())
                {
                    unsafe
                    {
                        byte* bodyIndexDataPointer = (byte*)bodyIndexData.UnderlyingBuffer;
                        int colorMappedToDepthPointCount = this.colorMappedToDepthPoints.Length;
                        fixed (DepthSpacePoint* colorMappedToDepthPointsPointer = this.colorMappedToDepthPoints)
                        {
                            // Treat the color data as 4-byte pixels
                            uint* bitmapPixelsPointer = (uint*)this.bitmap.BackBuffer;
                            // Loop over each row and column of the color image
                            // Zero out any pixels that don't correspond to a body index
                            for (int colorIndex = 0; colorIndex < colorMappedToDepthPointCount; ++colorIndex)
                            {
                                float colorMappedToDepthX = colorMappedToDepthPointsPointer[colorIndex].X;
                                float colorMappedToDepthY = colorMappedToDepthPointsPointer[colorIndex].Y;
                                // The sentinel value is -inf, -inf, meaning that no depth pixel corresponds to this color pixel.
                                if (!float.IsNegativeInfinity(colorMappedToDepthX) &&
                                    !float.IsNegativeInfinity(colorMappedToDepthY))
                                {
                                    // Make sure the depth pixel maps to a valid point in color space
                                    int depthX = (int)( colorMappedToDepthX + 0.5f );
                                    int depthY = (int)( colorMappedToDepthY + 0.5f );
                                    // If the point is not valid, there is no body index there.
                                    if (( depthX >= 0 ) && ( depthX < depthWidth ) && ( depthY >= 0 ) && ( depthY < depthHeight ))
                                    {
                                        int depthIndex = ( depthY * depthWidth ) + depthX;
                                        // If we are tracking a body for the current pixel, do not zero out the pixel
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
                test.Source = bitmap;
            }
        }

        public Rect GetBounds (FrameworkElement of, FrameworkElement from)
        {
            // Might throw an exception if of and from are not in the same visual tree
            GeneralTransform transform = of.TransformToVisual(from);

            return transform.TransformBounds(new Rect(0, 0, of.ActualWidth, of.ActualHeight));
        }

        
        private void kinectCoreWindow_PointerMoved (object sender, KinectPointerEventArgs args)
        {
            KinectPointerPoint point = args.CurrentPoint;
            if (lastTime == TimeSpan.Zero || lastTime != point.Properties.BodyTimeCounter)
            {
                lastTime = point.Properties.BodyTimeCounter;
            }

            Point screenRelative = new Point(
            point.Position.X * canvas.ActualWidth,
            point.Position.Y * canvas.ActualHeight);
            RenderPointer(point.Position, canvas, point.Properties.HandType);
            var i = 0;
            foreach (var relativeTo in pianoKeys)
            {
                Point relativeToElement = canvas.TranslatePoint(screenRelative, relativeTo);
                Boolean insideElement = (
                    relativeToElement.X >= 0 &&
                    relativeToElement.X < relativeTo.ActualWidth &&
                    relativeToElement.Y >= 0 &&
                    relativeToElement.Y < relativeTo.ActualHeight );
                if (point.Properties.HandType == HandType.RIGHT && insideElement)
                {
                    if (trackingBody.HandRightState == HandState.Lasso)
                        switch (i)
                        {
                            case 0:
                                ( (PianoKeyWPF)PianoControl.cnvPiano.Children[12] ).PressPianoKey();
                                break;
                            case 1:
                                ( (PianoKeyWPF)PianoControl.cnvPiano.Children[14] ).PressPianoKey();
                                break;
                            case 2:
                                ( (PianoKeyWPF)PianoControl.cnvPiano.Children[16] ).PressPianoKey();
                                break;
                            case 3:
                                ( (PianoKeyWPF)PianoControl.cnvPiano.Children[17] ).PressPianoKey();
                                break;
                            case 4:
                                ( (PianoKeyWPF)PianoControl.cnvPiano.Children[19] ).PressPianoKey();
                                break;
                            case 5:
                                ( (PianoKeyWPF)PianoControl.cnvPiano.Children[21] ).PressPianoKey();
                                break;
                            case 6:
                                break;
                            case 7:
                                break;
                            default:
                                break;
                        }
                }
                else if (point.Properties.HandType == HandType.LEFT && insideElement)
                {

                }
                i++;
            }
        }

        private void RenderPointer (PointF position, Canvas canvas, HandType handType)
        {
            StackPanel cursor = null;
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
            
            cursorEllipse.Fill = ( handType == HandType.LEFT) ? Brushes.Green : Brushes.Yellow;
            Canvas.SetLeft(cursor, position.X * canvas.ActualWidth - dotWidth / 2);
            Canvas.SetTop(cursor, position.Y * canvas.ActualHeight - dotHeight / 2);
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