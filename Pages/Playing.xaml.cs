using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
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
using AirBand.Controls;
using Microsoft.Kinect;
using Microsoft.Kinect.Input;
using Microsoft.Kinect.Wpf.Controls;
using Sanford.Multimedia.Midi;

namespace AirBand.Pages
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
        private Int32 trackingIndex;
        private Body[] bodyTrackingArray;
        private List<BodyViewModel> bodyList;
        private WriteableBitmap bitmap = null;
        private MultiSourceFrameReader reader;
        private FrameDescription colorFrameDesc;
        private KinectCoreWindow kinectCoreWindow;
        private System.Windows.Forms.Timer debounceTimer = new System.Windows.Forms.Timer() { Interval = 1000 };

        public Playing (EnvironmentVariablesViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
            outDevice = new OutputDevice(0);
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
                colorFrameDesc = null;
                bitmap = null;
                Image_BackgroundRemoval.Source = null;
                disposed = true;
                isLoaded = false;
            }
        }

        public void OutDeviceDispose ()
        {
            outDevice.Close();
            outDevice.Dispose();
        }

        #endregion

        private void Playing_Loaded (object sender, RoutedEventArgs e)
        {
            Grid_Main.Opacity = 0;
            sensor = KinectSensor.GetDefault();
            sensor.Open();
            bitmap = new WriteableBitmap(1920, 1080, 96.0, 96.0, PixelFormats.Bgra32, null);
            reader = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Body | FrameSourceTypes.Depth | FrameSourceTypes.BodyIndex);
            Image_Background.Visibility = System.Windows.Visibility.Collapsed;
            kinectCoreWindow = KinectCoreWindow.GetForCurrentThread();
            KinectCoreWindow.SetKinectTwoPersonSystemEngagement();
            kinectCoreWindow.PointerMoved += kinectCoreWindow_PointerMoved;
            bodyTrackingArray = new Body[2];
            bodyList = new List<BodyViewModel>();
            for (int i = 0; i < sensor.BodyFrameSource.BodyCount; ++i)
            {
                bodyList.Add(new BodyViewModel(i));
            }
            reader.MultiSourceFrameArrived += reader_MultiSourceFrameArrived;
            colorFrameDesc = sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            debounceTimer.Enabled = false;
            Image_BackgroundRemoval.Source = bitmap;
            StoryboardHandler.InitHitStoryBoard(this, "EnterStoryboard");

            disposed = false;
            isLoaded = true;
        }

        private void reader_MultiSourceFrameArrived (object sender, MultiSourceFrameArrivedEventArgs e)
        {
            MultiSourceFrame multiSourceFrame = e.FrameReference.AcquireFrame();
            using (var frame = multiSourceFrame.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    bodies = new Body[frame.BodyFrameSource.BodyCount];

                    frame.GetAndRefreshBodyData(bodies);

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
                                ( (Grid)body.Instrument.Tag ).Children.Remove(body.Instrument);
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

            using (ColorFrame colorFrame = multiSourceFrame.ColorFrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    FrameDescription colorFrameDescription = colorFrame.FrameDescription;
                    using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                    {
                        this.bitmap.Lock();
                        if (( colorFrameDescription.Width == this.bitmap.PixelWidth ) && ( colorFrameDescription.Height == this.bitmap.PixelHeight ))
                        {
                            colorFrame.CopyConvertedFrameDataToIntPtr(
                                this.bitmap.BackBuffer,
                                (uint)( colorFrameDescription.Width * colorFrameDescription.Height * 4 ),
                                ColorImageFormat.Bgra);
                            this.bitmap.AddDirtyRect(new Int32Rect(0, 0, this.bitmap.PixelWidth, this.bitmap.PixelHeight));
                        }
                        this.bitmap.Unlock();
                    }
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
            if (bodyTrackingArray == null)
                return;
            if (bodyTrackingArray[0] == null && bodyTrackingArray[1] == null)
                return;
            if (args.CurrentPoint.Properties.HandType != HandType.RIGHT)
                return;

            KinectPointerPoint point = args.CurrentPoint;
            UInt64 trackingId = point.Properties.BodyTrackingId;

            if (lastTime == TimeSpan.Zero || lastTime != point.Properties.BodyTimeCounter)
            {
                lastTime = point.Properties.BodyTimeCounter;
            }

            Int32 bodyIndex = ( bodyTrackingArray[1] != null && bodyTrackingArray[1].TrackingId == trackingId ) ? 1 : 0;
            Point screenRelative = new Point(
            point.Position.X * Canvas_Pointer.ActualWidth,
            point.Position.Y * Canvas_Pointer.ActualHeight);
            if (point.Properties.IsEngaged)
            {
                if (bodyIndex == 0)
                    PointerFirst.Visibility = System.Windows.Visibility.Visible;
                else
                    PointerSecond.Visibility = System.Windows.Visibility.Visible;
                RenderPointer(point.Position, Canvas_Pointer, bodyIndex);
                performLassoClick(screenRelative, trackingId, Button_Cheer);
                performLassoClick(screenRelative, trackingId, Button_Boo);
                performLassoClick(screenRelative, trackingId, Button_Mask);
                performLassoClick(screenRelative, trackingId, Button_Piano);
                performLassoClick(screenRelative, trackingId, Button_Guitar);
                performLassoClick(screenRelative, trackingId, Button_RandomEffect);
                performLassoClick(screenRelative, trackingId, Button_Clear);
            }
            else
            {
                if (bodyIndex == 0)
                    PointerFirst.Visibility = System.Windows.Visibility.Collapsed;
                else
                    PointerSecond.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void RenderPointer (PointF position, Canvas canvas, Int32 bodyIndex)
        {
            Double y = position.Y * canvas.ActualHeight;
            y = ( y > 720 ) ? 720 : y;
            Canvas.SetLeft(( bodyIndex == 0 ) ? PointerFirst : PointerSecond, position.X * canvas.ActualWidth - 25);
            Canvas.SetTop(( bodyIndex == 0 ) ? PointerFirst : PointerSecond, position.Y * canvas.ActualHeight - 35);
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
                    //VisualStateManager.GoToState(clickedButton, "Pressed", true);
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
                case "Button_RandomEffect":
                    Button_RandomEffect.IsHitTestVisible = false;
                    if (new Random().Next(2) < 1)
                    {
                        new SoundPlayer(Application.GetResourceStream(new Uri("/AirBand;component/Resources/Cheer.wav", UriKind.Relative)).Stream).Play();
                        Image_Effect.Source = new BitmapImage(new Uri("/AirBand;component/Resources/CheerEffect.png", UriKind.Relative));
                        StoryboardHandler.InitStoryBoard(this, "CheerEffectStoryboard", () => Button_RandomEffect.IsHitTestVisible = true);
                    }
                    else
                    {
                        new SoundPlayer(Application.GetResourceStream(new Uri("/AirBand;component/Resources/Boo.wav", UriKind.Relative)).Stream).Play();
                        Image_Effect.Source = new BitmapImage(new Uri("/AirBand;component/Resources/BooEffect.png", UriKind.Relative));
                        StoryboardHandler.InitStoryBoard(this, "BooEffectStoryboard", () => Button_RandomEffect.IsHitTestVisible = true);
                    }
                    break;
                case "Button_Cheer":
                    Button_Cheer.IsHitTestVisible = false;
                    new SoundPlayer(Application.GetResourceStream(new Uri("/AirBand;component/Resources/Cheer.wav", UriKind.Relative)).Stream).Play();
                        Image_Effect.Source = new BitmapImage(new Uri("/AirBand;component/Resources/CheerEffect.png", UriKind.Relative));
                        StoryboardHandler.InitStoryBoard(this, "CheerEffectStoryboard", () => Button_Cheer.IsHitTestVisible = true);
                    break;
                case "Button_Boo":
                    Button_Boo.IsHitTestVisible = false;
                    new SoundPlayer(Application.GetResourceStream(new Uri("/AirBand;component/Resources/Boo.wav", UriKind.Relative)).Stream).Play();
                        Image_Effect.Source = new BitmapImage(new Uri("/AirBand;component/Resources/BooEffect.png", UriKind.Relative));
                        StoryboardHandler.InitStoryBoard(this, "BooEffectStoryboard", () => Button_Boo.IsHitTestVisible = true);
                    break;
                case "Button_Mask":
                    Button_Mask.IsHitTestVisible = false;
                    if (body != null && body.Mask == null)
                    {
                        body.SetMask(new Image() { Source = Switcher.viewModel.Mask });
                        Canvas_Mask.Children.Add(body.Mask);
                        //new SoundPlayer(Application.GetResourceStream(new Uri("/AirBand;component/Resources/Maji.wav", UriKind.Relative)).Stream).Play();
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
                case "Button_Piano":
                    if (body != null && ( body.Instrument == null || body.Instrument.GetType() != typeof(PianoControl) ))
                    {
                        if (body.Instrument != null)
                            ( (Grid)body.Instrument.Tag ).Children.Remove(body.Instrument);
                        body.ClearInstrument();
                        body.SetInstrument(new PianoControl(outDevice, body, Grid_PianoControls));
                        Grid_PianoControls.Children.Add(body.Instrument);
                    }
                    break;
                case "Button_Guitar":
                    if (body != null && ( body.Instrument == null || body.Instrument.GetType() != typeof(GuitarControl) ))
                    {
                        if (body.Instrument != null)
                            ( (Grid)body.Instrument.Tag ).Children.Remove(body.Instrument);
                        body.ClearInstrument();
                        body.SetInstrument(new GuitarControl(outDevice, body, Grid_GuitarControls));
                        Grid_GuitarControls.Children.Add(body.Instrument);
                    }
                    break;
                case "Button_Clear":
                    if (body != null && body.Instrument != null)
                    {
                        ( (Grid)body.Instrument.Tag ).Children.Remove(body.Instrument);
                        body.ClearInstrument();
                    }
                    break;
                case "Button_Back":
                    Canvas_Mask.Children.Clear();
                    Grid_BackPianoControls.Children.Clear();
                    Grid_PianoControls.Children.Clear();
                    Grid_GuitarControls.Children.Clear();
                    Dispose(true);
                    StoryboardHandler.InitHitStoryBoard(this, "ExitStoryboard", () => Switcher.Switch("MainMenu"));
                    break;
                default:
                    break;
            }
        }
    }
}