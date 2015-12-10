using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AirBand.Controls;
using Microsoft.Kinect;
using Microsoft.Kinect.Input;
using MyoSharp.Communication;
using MyoSharp.ConsoleSample.Internal;
using MyoSharp.Device;
using MyoSharp.Exceptions;
using MyoSharp.Poses;
using Sanford.Multimedia.Midi;

namespace AirBand.Pages
{
    public partial class BRPlaying : UserControl, IDisposable
    {
        private bool disposed = false;
        private bool isLoaded = false;
        private readonly int bytesPerPixel = ( PixelFormats.Bgr32.BitsPerPixel + 7 ) / 8;
        private KinectSensor sensor = null;
        private CoordinateMapper coordinateMapper = null;
        private MultiSourceFrameReader reader = null;
        private TimeSpan lastTime;
        private int trackingIndex;
        private WriteableBitmap bitmap = null;
        private uint bitmapBackBufferSize = 0;
        private DepthSpacePoint[] colorMappedToDepthPoints = null;
        private bool isSelectMode = false;
        private WriteableBitmap bodyIndexBitmap = null;
        private KinectCoreWindow kinectCoreWindow;
        private OutputDevice outDevice;
        private bool engagementPeopleHaveChanged = false;
        private List<BodyHandPair> handsToEngage;
        private Body[] bodies;
        private FrameDescription bodyIndexFrameDescription = null;
        private Body[] bodyTrackingArray;
        private List<BodyViewModel> bodyList;
        private uint[] bodyIndexPixels = null;
        private System.Windows.Forms.Timer debounceTimer = new System.Windows.Forms.Timer() { Interval = 1500 };
        private ulong trackingBodyID = 0;
        private int trackingInsID = 0;
        private static readonly uint[] BodyColor =
        {
            0x0000FF00,
            0x00FF0000,
            0xFFFF4000,
            0x40FFFF00,
            0xFF40FF00,
            0xFF808000,
        };
        private IHeldPose pose;
        private IHeldPose rotatePose;
        private IMyo myo;

        public BRPlaying (EnvironmentVariablesViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            var channel = Channel.Create(
            ChannelDriver.Create(ChannelBridge.Create(),
            MyoErrorHandlerDriver.Create(MyoErrorHandlerBridge.Create())));
            var hub = Hub.Create(channel);
            hub.MyoConnected += (sender, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    myo = e.Myo;

                    pose = HeldPose.Create(e.Myo, Pose.DoubleTap);
                    pose.Triggered += Pose_Triggered;
                    pose.Interval = TimeSpan.FromSeconds(0.5);
                    pose.Start();
                    rotatePose = HeldPose.Create(e.Myo, Pose.Fist);
                    rotatePose.Triggered += RotatePose_Triggered;
                    rotatePose.Interval = TimeSpan.FromSeconds(0.8);
                    var sequence = PoseSequence.Create(e.Myo, Pose.WaveOut, Pose.WaveIn);
                    sequence.PoseSequenceCompleted += Sequence_PoseSequenceCompleted;
                });
            };
            channel.StartListening();
            ConsoleHelper.UserInputLoop(hub);
        }

        ~BRPlaying()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (isLoaded)
            {
                kinectCoreWindow.PointerMoved -= kinectCoreWindow_PointerMoved;
                reader.MultiSourceFrameArrived -= reader_MultiSourceFrameArrived;
                if (disposing && isLoaded && reader != null)
                {
                    reader.Dispose();
                    reader = null;
                }
                if (disposing && isLoaded && sensor != null)
                {
                    sensor.Close();
                    sensor = null;
                }
                bodies = null;
                kinectCoreWindow = null;
                bodyTrackingArray = null;
                bitmap = null;
                Image_BackgroundRemoval.Source = null;
                disposed = true;
                isLoaded = false;
            }
        }

        public void OutDeviceDispose()
        {
            if (outDevice == null)
                return;
            outDevice.Close();
            outDevice.Dispose();
            outDevice = null;
        }

        private void Sequence_PoseSequenceCompleted(object sender, PoseSequenceEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Canvas_Mask.Children.Clear();
                Grid_PianoControls.Children.Clear();
                Grid_GuitarControls.Children.Clear();
                Grid_DrumControls.Children.Clear();
                if (myo != null)
                    myo.Lock();
                Dispose(true);
                StoryboardHandler.InitHitStoryBoard(this, "ExitStoryboard", () => Switcher.Switch("MainMenu"));
            });
        }

        void setPos(double top, double left, UIElement element)
        {
            Canvas.SetTop(element, top);
            Canvas.SetLeft(element, left);
        }

        private void Pose_Triggered(object sender, PoseEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (!isSelectMode)
                {
                    StoryboardHandler.InitNotHitStoryBoard(this, "Select", () => 
                    {
                        rotatePose.Start();
                        isSelectMode = true;
                    });
                }
                else
                {
                    StoryboardHandler.InitNotHitStoryBoard(this, "Selected", () =>
                    {
                        rotatePose.Stop();
                        isSelectMode = false;
                        equipIns(trackingInsID);
                    });
                }
            });
        }

        private void equipIns(int id)
        {
            BodyViewModel body = bodyList.Where(x => x.TrackingId == Convert.ToUInt64(trackingBodyID)).First();
            switch (id)
            {
                case 0:
                    if (body != null && (body.Instrument == null || body.Instrument.GetType() != typeof(PianoControl)))
                    {
                        if (body.Instrument != null)
                            ((Grid)body.Instrument.Tag).Children.Remove(body.Instrument);
                        body.ClearInstrument();
                        body.SetInstrument(new PianoControl(outDevice, body, Grid_PianoControls));
                        Grid_PianoControls.Children.Add(body.Instrument);
                    }
                    break;
                case 1:
                    if (body != null && (body.Instrument == null || body.Instrument.GetType() != typeof(GuitarControl)))
                    {
                        if (body.Instrument != null)
                            ((Grid)body.Instrument.Tag).Children.Remove(body.Instrument);
                        body.ClearInstrument();
                        body.SetInstrument(new GuitarControl(outDevice, body, Grid_GuitarControls));
                        Grid_GuitarControls.Children.Add(body.Instrument);
                    }
                    break;
                case 2:
                    if (body != null && (body.Instrument == null || body.Instrument.GetType() != typeof(DrumControl)))
                    {
                        if (body.Instrument != null)
                            ((Grid)body.Instrument.Tag).Children.Remove(body.Instrument);
                        body.ClearInstrument();
                        body.SetInstrument(new DrumControl(outDevice, body, Grid_DrumControls));
                        Grid_DrumControls.Children.Add(body.Instrument);
                    }
                    break;
                default:
                    break;
            }
        }

        private void RotatePose_Triggered(object sender, PoseEventArgs e)
        {
            const float PI = (float)Math.PI;
            const int sd = 38;
            var id = (int)((e.Myo.Orientation.Z + PI) / (PI * 2.0f) * 100);
            Dispatcher.Invoke(() =>
            {
                VisualStateManager.GoToState(Button_Violin, "Normal", false);
                VisualStateManager.GoToState(Button_Piano, "Normal", false);
                VisualStateManager.GoToState(Button_Drum, "Normal", false);
                VisualStateManager.GoToState(Button_Guitar, "Normal", false);
                VisualStateManager.GoToState(Button_Flute, "Normal", false);

                if (id >= sd+3)
                {
                    VisualStateManager.GoToState(Button_Violin, "MouseOver", false);
                }
                else if (id <= sd+2 && id >= sd+1)
                {
                    VisualStateManager.GoToState(Button_Piano, "MouseOver", false);
                    trackingInsID = 0;
                }
                else if (id <= sd && id >=sd-1)
                {
                    VisualStateManager.GoToState(Button_Drum, "MouseOver", false);
                    trackingInsID = 2;
                }
                else if (id <= sd-2 && id >= sd-3)
                {
                    VisualStateManager.GoToState(Button_Guitar, "MouseOver", false);
                    trackingInsID = 1;
                }
                else if (id <= sd-4)
                {
                    VisualStateManager.GoToState(Button_Flute, "MouseOver", false);
                }
            });
        }

        private void BRPlaying_Loaded (object se, RoutedEventArgs ev)
        {
            ((Playing)Switcher.PageDictionary["Playing"]).OutDeviceDispose();
            if (outDevice == null)
            {
                outDevice = new OutputDevice(0);
            }

            if (myo != null)
                myo.Unlock(UnlockType.Hold);
            Grid_Main.Opacity = 0;
            canvas.Opacity = 0;
            canvas.RenderTransform = new RotateTransform(-60);
            setPos(320, 619, Button_Drum);
            setPos(320, 619, Button_Guitar);
            setPos(320, 619, Button_Flute);
            setPos(320, 619, Button_Violin);
            setPos(320, 619, Button_Piano);

            sensor = KinectSensor.GetDefault();

            reader = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Depth | FrameSourceTypes.Color | FrameSourceTypes.BodyIndex | FrameSourceTypes.Body);

            reader.MultiSourceFrameArrived += reader_MultiSourceFrameArrived;

            coordinateMapper = sensor.CoordinateMapper;

            FrameDescription depthFrameDescription = sensor.DepthFrameSource.FrameDescription;

            int depthWidth = depthFrameDescription.Width;
            int depthHeight = depthFrameDescription.Height;

            FrameDescription colorFrameDescription = sensor.ColorFrameSource.FrameDescription;

            int colorWidth = colorFrameDescription.Width;
            int colorHeight = colorFrameDescription.Height;

            colorMappedToDepthPoints = new DepthSpacePoint[colorWidth * colorHeight];

            bitmap = new WriteableBitmap(colorWidth, colorHeight, 96.0, 96.0, PixelFormats.Bgra32, null);
            kinectCoreWindow = KinectCoreWindow.GetForCurrentThread();
            kinectCoreWindow.PointerMoved += kinectCoreWindow_PointerMoved;
            bodyTrackingArray = new Body[2];
            bodyList = new List<BodyViewModel>();
            handsToEngage = new List<BodyHandPair>();
            for (int i = 0; i < sensor.BodyFrameSource.BodyCount; ++i)
            {
                bodyList.Add(new BodyViewModel(i));
            }
            debounceTimer.Enabled = false;
            bitmapBackBufferSize = (uint)( ( bitmap.BackBufferStride * ( bitmap.PixelHeight - 1 ) ) + ( bitmap.PixelWidth * bytesPerPixel ) );
            bodyIndexFrameDescription = sensor.BodyIndexFrameSource.FrameDescription;
            bodyIndexPixels = new uint[bodyIndexFrameDescription.Width * bodyIndexFrameDescription.Height];
            bodyIndexBitmap = new WriteableBitmap(bodyIndexFrameDescription.Width, bodyIndexFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
            Image_BackgroundRemoval.Source = bitmap;
            Image_UserView.Source = bodyIndexBitmap;
            sensor.Open();

            StoryboardHandler.InitHitStoryBoard(this, "EnterStoryboard");
            disposed = false;
            isLoaded = true;
        }

        private void RenderPointer(PointF position, Canvas canvas, int bodyIndex)
        {
            double y = position.Y * canvas.ActualHeight;
            y = (y > 720) ? 720 : y;
            Canvas.SetLeft((bodyIndex == 0) ? PointerFirst : PointerSecond, position.X * canvas.ActualWidth - 25);
            Canvas.SetTop((bodyIndex == 0) ? PointerFirst : PointerSecond, position.Y * canvas.ActualHeight - 35);
        }

        private void kinectCoreWindow_PointerMoved(object sender, KinectPointerEventArgs args)
        {
            if (bodyTrackingArray == null)
                return;
            if (bodyTrackingArray[0] == null && bodyTrackingArray[1] == null)
                return;
            if (args.CurrentPoint.Properties.HandType != HandType.RIGHT)
                return;

            KinectPointerPoint point = args.CurrentPoint;
            ulong trackingId = point.Properties.BodyTrackingId;
            trackingBodyID = trackingId;

            if (lastTime == TimeSpan.Zero || lastTime != point.Properties.BodyTimeCounter)
            {
                lastTime = point.Properties.BodyTimeCounter;
            }

            int bodyIndex = (bodyTrackingArray[1] != null && bodyTrackingArray[1].TrackingId == trackingId) ? 1 : 0;
            Point screenRelative = new Point(
            point.Position.X * Canvas_Pointer.ActualWidth,
            point.Position.Y * Canvas_Pointer.ActualHeight);
            if (point.Properties.IsEngaged)
            {
                if (bodyIndex == 0)
                    PointerFirst.Visibility = Visibility.Visible;
                else
                    PointerSecond.Visibility = Visibility.Visible;
                RenderPointer(point.Position, Canvas_Pointer, bodyIndex);
                performLassoClick(screenRelative, trackingId, Button_Cheer);
                performLassoClick(screenRelative, trackingId, Button_Boo);
                performLassoClick(screenRelative, trackingId, Button_Mask);
            }
            else
            {
                if (bodyIndex == 0)
                    PointerFirst.Visibility = Visibility.Collapsed;
                else
                    PointerSecond.Visibility = Visibility.Collapsed;
            }
        }

        private void performLassoClick(Point relative, ulong trackingId, UIElement relativeTo)
        {
            Button clickedButton = relativeTo as Button;
            if (!clickedButton.IsHitTestVisible)
                return;

            if (clickedButton != null)
            {
                Point relativeToElement = Canvas_Pointer.TranslatePoint(relative, clickedButton);
                bool insideElement = (relativeToElement.X >= 0 && relativeToElement.X < clickedButton.ActualWidth
                    && relativeToElement.Y >= 0 && relativeToElement.Y < clickedButton.ActualHeight);
                bool isLasso = bodyList.Where(x => x.TrackingId == trackingId).First().RightHandState == HandState.Lasso;
                if (insideElement)
                {
                    VisualStateManager.GoToState(clickedButton, "MouseOver", false);
                    if (isLasso)
                    {
                        clickedButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, trackingId));
                    }
                }
                else
                {
                    VisualStateManager.GoToState(clickedButton, "Normal", true);
                }
            }
        }

        private void BRPlaying_Unloaded (object sender, RoutedEventArgs e)
        {
            if (reader != null)
            {
                reader.Dispose();
                reader = null;
            }

            if (sensor != null)
            {
                sensor.Close();
                sensor = null;
            }
        }

        private void TrackEngagedPlayersViaHandOverHead()
        {
            engagementPeopleHaveChanged = false;
            var currentlyEngagedHands = KinectCoreWindow.KinectManualEngagedHands;
            handsToEngage.Clear();

            // check to see if anybody who is currently engaged should be disengaged
            foreach (var bodyHandPair in currentlyEngagedHands)
            {
                var bodyTrackingId = bodyHandPair.BodyTrackingId;
                foreach (var body in bodies)
                {
                    if (body.TrackingId == bodyTrackingId)
                    {
                        // check for disengagement
                        bool toBeDisengaged = (body.Joints[JointType.HandRight].Position.Y < body.Joints[JointType.SpineBase].Position.Y);

                        if (toBeDisengaged)
                        {
                            engagementPeopleHaveChanged = true;
                        }
                        else
                        {
                            handsToEngage.Add(bodyHandPair);
                        }
                    }
                }
            }

            // check to see if anybody should be engaged, if not already engaged
            foreach (var body in bodies)
            {
                if (handsToEngage.Count < 2)
                {
                    bool alreadyEngaged = false;
                    foreach (var bodyHandPair in this.handsToEngage)
                    {
                        alreadyEngaged = (body.TrackingId == bodyHandPair.BodyTrackingId);
                    }

                    if (!alreadyEngaged && IsHandOverhead(JointType.HandRight, body))
                    {
                        // engage the right hand
                        handsToEngage.Add(new BodyHandPair(body.TrackingId, HandType.RIGHT));
                        engagementPeopleHaveChanged = true;
                    }
                }
            }

            if (this.engagementPeopleHaveChanged)
            {
                BodyHandPair firstPersonToEngage = null;
                BodyHandPair secondPersonToEngage = null;

                switch (this.handsToEngage.Count)
                {
                    case 0:
                        break;
                    case 1:
                        firstPersonToEngage = this.handsToEngage[0];
                        break;
                    case 2:
                        firstPersonToEngage = this.handsToEngage[0];
                        secondPersonToEngage = this.handsToEngage[1];
                        break;
                }

                KinectCoreWindow.SetKinectOnePersonManualEngagement(firstPersonToEngage);
            }
        }

        private void RenderBodyIndexPixels()
        {
            bodyIndexBitmap.WritePixels(
                new Int32Rect(0, 0, bodyIndexBitmap.PixelWidth, bodyIndexBitmap.PixelHeight),
                bodyIndexPixels,
                bodyIndexBitmap.PixelWidth * 4,
                0);

        }

        private Point coordinateMap(CameraSpacePoint position)
        {
            var point = sensor.CoordinateMapper.MapCameraPointToColorSpace(position);
            return new Point(
                float.IsInfinity(point.X) ? 0 : point.X / 1920 * Canvas_Pointer.ActualWidth,
                float.IsInfinity(point.Y) ? 0 : point.Y / 1080 * Canvas_Pointer.ActualHeight);
        }

        private static bool IsHandOverhead(JointType jointType, Body body)
        {
            return (body.Joints[jointType].Position.Y > body.Joints[JointType.Head].Position.Y);
        }

        private void reader_MultiSourceFrameArrived (object sender, MultiSourceFrameArrivedEventArgs e)
        {
            int depthWidth = 0;
            int depthHeight = 0;

            DepthFrame depthFrame = null;
            ColorFrame colorFrame = null;
            BodyIndexFrame bodyIndexFrame = null;
            bool isBitmapLocked = false;

            MultiSourceFrame multiSourceFrame = e.FrameReference.AcquireFrame();

            if (multiSourceFrame == null)
                return;
            
            try
            {
                depthFrame = multiSourceFrame.DepthFrameReference.AcquireFrame();
                colorFrame = multiSourceFrame.ColorFrameReference.AcquireFrame();
                bodyIndexFrame = multiSourceFrame.BodyIndexFrameReference.AcquireFrame();
                
                if (( depthFrame == null ) || ( colorFrame == null ) || ( bodyIndexFrame == null ))
                    return;

                using (var frame = multiSourceFrame.BodyFrameReference.AcquireFrame())
                {
                    if (frame != null)
                    {
                        bodies = new Body[frame.BodyFrameSource.BodyCount];

                        frame.GetAndRefreshBodyData(bodies);

                        TrackEngagedPlayersViaHandOverHead();

                        trackingIndex = 0;

                        foreach (var item in bodies.Select((value, i) => new { i, value }))
                        {
                            BodyViewModel body = bodyList[item.i];
                            body.SetBodyData(item.value);

                            if (body.TrackingId != 0 && trackingIndex < 2)
                            {
                                bodyTrackingArray[trackingIndex] = item.value;
                                trackingIndex++;
                            }

                            if (item.value.IsTracked)
                            {
                                //2維頭心點
                                Point headPoint = coordinateMap(item.value.Joints[JointType.Head].Position);
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
                                //左手狀態程度
                                TrackingConfidence leftHandConfidence = item.value.HandLeftConfidence;
                                //右手狀態
                                HandState rightHandState = item.value.HandRightState;
                                //右手狀態程度
                                TrackingConfidence rightHandConfidence = item.value.HandRightConfidence;
                                //更新體感數據
                                body.UpdateBodyData(
                                    headPoint,
                                    centerPoint, spinePoint,
                                    locatePoint, leftVariabPoint,
                                    rightVariabPoint, shouldPoint,
                                    leftHandState, leftHandConfidence,
                                    rightHandState, rightHandConfidence);
                                if (body.Instrument != null)
                                    body.UpdateInstrument();
                                if (body.Mask != null)
                                    body.UpdateMask();
                            }
                            else
                            {
                                if (body.Instrument != null)
                                {
                                    ((Grid)body.Instrument.Tag).Children.Remove(body.Instrument);
                                    body.ClearInstrument();
                                }
                                if (body.Mask != null)
                                {
                                    Canvas_Mask.Children.Remove(body.Mask);
                                    body.ClearMask();
                                }
                            }
                        }
                    }
                }

                FrameDescription depthFrameDescription = depthFrame.FrameDescription;
                depthWidth = depthFrameDescription.Width;
                depthHeight = depthFrameDescription.Height;

                using (KinectBuffer depthFrameData = depthFrame.LockImageBuffer())
                {
                    this.coordinateMapper.MapColorFrameToDepthSpaceUsingIntPtr(
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
                    if (((bodyIndexFrameDescription.Width * bodyIndexFrameDescription.Height) == bodyIndexData.Size) &&
                            (bodyIndexFrameDescription.Width == bodyIndexBitmap.PixelWidth) && (bodyIndexFrameDescription.Height == this.bodyIndexBitmap.PixelHeight))
                    {
                        ProcessBodyIndexFrameData(bodyIndexData.UnderlyingBuffer, bodyIndexData.Size);
                        RenderBodyIndexPixels();
                    }
                    unsafe
                    {
                        byte* bodyIndexDataPointer = (byte*)bodyIndexData.UnderlyingBuffer;

                        int colorMappedToDepthPointCount = colorMappedToDepthPoints.Length;

                        fixed (DepthSpacePoint* colorMappedToDepthPointsPointer = colorMappedToDepthPoints)
                        {
                            uint* bitmapPixelsPointer = (uint*)bitmap.BackBuffer;

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
                                            continue;
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
                    bitmap.Unlock();
                if (depthFrame != null)
                    depthFrame.Dispose();
                if (colorFrame != null)
                    colorFrame.Dispose();
                if (bodyIndexFrame != null)
                    bodyIndexFrame.Dispose();
            }
        }

        private unsafe void ProcessBodyIndexFrameData(IntPtr bodyIndexFrameData, uint bodyIndexFrameDataSize)
        {
            byte* frameData = (byte*)bodyIndexFrameData;
            for (int i = 0; i < (int)bodyIndexFrameDataSize; ++i)
            {
                if (frameData[i] < BodyColor.Length)
                {
                    bodyIndexPixels[i] = BodyColor[frameData[i]];
                }
                else
                {
                    bodyIndexPixels[i] = 0x00000000;
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
                case "Button_Cheer":
                    Button_Cheer.IsHitTestVisible = false;
                    Button_Boo.IsHitTestVisible = false;
                    new SoundPlayer(Application.GetResourceStream(new Uri("/AirBand;component/Resources/Cheer.wav", UriKind.Relative)).Stream).Play();
                    Image_Effect.Source = new BitmapImage(new Uri("/AirBand;component/Resources/CheerEffect.png", UriKind.Relative));
                    StoryboardHandler.InitStoryBoard(this, "CheerEffectStoryboard", () =>
                    {
                        Button_Cheer.IsHitTestVisible = true;
                        Button_Boo.IsHitTestVisible = true;
                    });
                    break;
                case "Button_Boo":
                    Button_Boo.IsHitTestVisible = false;
                    Button_Cheer.IsHitTestVisible = false;
                    new SoundPlayer(Application.GetResourceStream(new Uri("/AirBand;component/Resources/Boo.wav", UriKind.Relative)).Stream).Play();
                    Image_Effect.Source = new BitmapImage(new Uri("/AirBand;component/Resources/BooEffect.png", UriKind.Relative));
                    StoryboardHandler.InitStoryBoard(this, "BooEffectStoryboard", () =>
                    {
                        Button_Boo.IsHitTestVisible = true;
                        Button_Cheer.IsHitTestVisible = true;
                    });
                    break;
                case "Button_Mask":
                    Button_Mask.IsHitTestVisible = false;
                    if (body != null && body.Mask == null)
                    {
                        body.SetMask(new Image() { Source = Switcher.viewModel.Mask });
                        Canvas_Mask.Children.Add(body.Mask);
                    }
                    else if (body != null && body.Mask != null)
                    {
                        Canvas_Mask.Children.Remove(body.Mask);
                        body.ClearMask();
                    }
                    debounceTimer.Enabled = true;
                    debounceTimer.Tick += (s, ev) =>
                    {
                        debounceTimer.Enabled = false;
                        Button_Mask.IsHitTestVisible = true;
                    };
                    break;
                case "Button_Back":
                    Canvas_Mask.Children.Clear();
                    Grid_PianoControls.Children.Clear();
                    Grid_GuitarControls.Children.Clear();
                    Grid_DrumControls.Children.Clear();
                    if (myo != null)
                        myo.Lock();
                    Dispose(true);
                    StoryboardHandler.InitHitStoryBoard(this, "ExitStoryboard", () => Switcher.Switch("MainMenu"));
                    break;
                default:
                    break;
            }
        }
    }
}