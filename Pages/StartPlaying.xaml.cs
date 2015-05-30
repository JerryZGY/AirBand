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
        private FrameDescription colorFrameDesc;
        private TimeSpan lastTime;
        private const double dotHeight = 30, dotWidth = 30;
        private List<PianoControlData> dataList;
        private PianoControlData[] trackingData;
        private List<ToneTriggerHandler> toneTriggerList;
        private DrawingGroup drawingGroup;
        private DrawingImage imageSource;
        private WriteableBitmap bitmap = null;
        Rectangle[] pianoKeys = new Rectangle[8];
        DepthSpacePoint[] colorMappedToDepthPoints = new DepthSpacePoint[1920 * 1080];
        private uint bitmapBackBufferSize;
        private Body trackingBody = null;

        //左方鍵渲染中心
        private Point leftRenderCenter = new Point(1, 1);
        //右方鍵渲染中心
        private Point rightRenderCenter = new Point(0, 1);

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
                colorFrameDesc = null;
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
            colorFrameDesc = sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            colorBitmap = new WriteableBitmap(colorFrameDesc.Width, colorFrameDesc.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
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
                    Width = 0,
                    Height = 0,
                };
                return x;
            }).ToArray();

            this.bitmap = new WriteableBitmap(1920, 1080, 96.0, 96.0, PixelFormats.Bgra32, null);
            // Calculate the WriteableBitmap back buffer size
            this.bitmapBackBufferSize = (uint)( ( this.bitmap.BackBufferStride * ( this.bitmap.PixelHeight - 1 ) ) + ( this.bitmap.PixelWidth * 4 ) );
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
                            //取得3維中心點
                            ColorSpacePoint centerColorPoint = sensor.CoordinateMapper.MapCameraPointToColorSpace(body.Joints[JointType.SpineMid].Position);
                            //取得3維定位點
                            ColorSpacePoint locateColorPoint = sensor.CoordinateMapper.MapCameraPointToColorSpace(body.Joints[JointType.Head].Position);
                            //取得3維變異點
                            ColorSpacePoint variabColorPoint = sensor.CoordinateMapper.MapCameraPointToColorSpace(body.Joints[JointType.HandRight].Position);
                            //取得3維左肩點
                            ColorSpacePoint shouldColorPoint = sensor.CoordinateMapper.MapCameraPointToColorSpace(body.Joints[JointType.SpineShoulder].Position);
                            //取得3維右肩點
                            ColorSpacePoint shouldRightColorPoint = sensor.CoordinateMapper.MapCameraPointToColorSpace(body.Joints[JointType.ShoulderRight].Position);
                            //使用coordinateMap函式將2維ColorFrame座標轉換至2維Canvas座標
                            //取得2維中心點
                            Point centerPoint = coordinateMap(centerColorPoint);
                            //取得2維定位點
                            Point locatePoint = coordinateMap(locateColorPoint);
                            //取得2維變異點
                            Point variabPoint = coordinateMap(variabColorPoint);
                            //取得2維左肩點
                            Point shouldPoint = coordinateMap(shouldColorPoint);
                            //取得2維右肩點
                            Point shouldRightPoint = coordinateMap(shouldRightColorPoint);
                            //自定義半徑 = 中心點距離 - 定位點距離 + 自定義常數
                            Double radius = Math.Abs(centerPoint.Y - locatePoint.Y + 100);
                            //自定義角度
                            Int32 angle = 90;
                            RES.Content =
                                IsPointInSector(centerPoint.X, centerPoint.Y, locatePoint.X, locatePoint.Y, variabPoint.X, variabPoint.Y, radius, angle) ?
                                "True" : "False";

                            var i = 0;
                            foreach (var key in pianoKeys)
                            {
                                key.Width = radius / 5;
                                key.Height = radius;
                                Canvas.SetTop(key, shouldPoint.Y - radius * 0.8);
                                switch (i)
                                {
                                    case 0:
                                        key.RenderTransform = new RotateTransform(-90);
                                        break;
                                    case 1:
                                        key.RenderTransform = new RotateTransform(-67.5);
                                        break;
                                    case 2:
                                        key.RenderTransform = new RotateTransform(-45);
                                        break;
                                    case 3:
                                        key.RenderTransform = new RotateTransform(-22.5);
                                        break;
                                    case 4:
                                        key.RenderTransform = new RotateTransform(22.5);
                                        break;
                                    case 5:
                                        key.RenderTransform = new RotateTransform(45);
                                        break;
                                    case 6:
                                        key.RenderTransform = new RotateTransform(67.5);
                                        break;
                                    case 7:
                                        key.RenderTransform = new RotateTransform(90);
                                        break;
                                    default:
                                        break;
                                }
                                if (i < 4)
                                {
                                    key.RenderTransformOrigin = leftRenderCenter;
                                    Canvas.SetLeft(key, centerPoint.X - radius * 0.2);
                                }
                                else
                                {
                                    key.RenderTransformOrigin = rightRenderCenter;
                                    Canvas.SetLeft(key, centerPoint.X - radius * 0.03);
                                }
                                canvas.Children.Add(key);
                                i++;
                            }

                            /*key0 =
                                new Rectangle() { Fill = Brushes.White, Width = radius / 5, Height = radius, RenderTransformOrigin = leftRenderCenter, RenderTransform = new RotateTransform(-90) };
                            key1 =
                                new Rectangle() { Fill = Brushes.White, Width = radius / 5, Height = radius, RenderTransformOrigin = leftRenderCenter, RenderTransform = new RotateTransform(-67.5) };
                            key2 =
                                new Rectangle() { Fill = Brushes.White, Width = radius / 5, Height = radius, RenderTransformOrigin = leftRenderCenter, RenderTransform = new RotateTransform(-45) };
                            key3 =
                                new Rectangle() { Fill = Brushes.White, Width = radius / 5, Height = radius, RenderTransformOrigin = leftRenderCenter, RenderTransform = new RotateTransform(-22.5) };
                            Canvas.SetLeft(key0, centerPoint.X - radius * 0.2);
                            Canvas.SetTop(key0, shouldPoint.Y - radius * 0.8);
                            canvas.Children.Add(key0);
                            Canvas.SetLeft(key1, centerPoint.X - radius * 0.2);
                            Canvas.SetTop(key1, shouldPoint.Y - radius * 0.8);
                            canvas.Children.Add(key1);
                            Canvas.SetLeft(key2, centerPoint.X - radius * 0.2);
                            Canvas.SetTop(key2, shouldPoint.Y - radius * 0.8);
                            canvas.Children.Add(key2);
                            Canvas.SetLeft(key3, centerPoint.X - radius * 0.2);
                            Canvas.SetTop(key3, shouldPoint.Y - radius * 0.8);
                            canvas.Children.Add(key3);

                            key4 =
                                new Rectangle() { Fill = Brushes.White, Width = radius / 5, Height = radius, RenderTransformOrigin = rightRenderCenter, RenderTransform = new RotateTransform(22.5) };
                            key5 =
                                new Rectangle() { Fill = Brushes.White, Width = radius / 5, Height = radius, RenderTransformOrigin = rightRenderCenter, RenderTransform = new RotateTransform(45) };
                            key6 =
                                new Rectangle() { Fill = Brushes.White, Width = radius / 5, Height = radius, RenderTransformOrigin = rightRenderCenter, RenderTransform = new RotateTransform(67.5) };
                            key7 =
                                new Rectangle() { Fill = Brushes.White, Width = radius / 5, Height = radius, RenderTransformOrigin = rightRenderCenter, RenderTransform = new RotateTransform(90) };
                            Canvas.SetLeft(key4, centerPoint.X - radius * 0.03);
                            Canvas.SetTop(key4, shouldPoint.Y - radius * 0.8);
                            canvas.Children.Add(key4);
                            Canvas.SetLeft(key5, centerPoint.X - radius * 0.03);
                            Canvas.SetTop(key5, shouldPoint.Y - radius * 0.8);
                            canvas.Children.Add(key5);
                            Canvas.SetLeft(key6, centerPoint.X - radius * 0.03);
                            Canvas.SetTop(key6, shouldPoint.Y - radius * 0.8);
                            canvas.Children.Add(key6);
                            Canvas.SetLeft(key7, centerPoint.X - radius * 0.03);
                            Canvas.SetTop(key7, shouldPoint.Y - radius * 0.8);
                            canvas.Children.Add(key7);*/

                            /*Ellipse testt = new Ellipse() { Fill = Brushes.Red, Width = 20, Height = 20 };
                            Canvas.SetLeft(testt, shouldPoint.X - radius / 12 - 10);
                            Canvas.SetTop(testt, shouldPoint.Y - radius - 10);
                            pointCanvas.Children.Add(testt);*/

                            Ellipse rightHand = new Ellipse() { Fill = Brushes.Red, Width = 20, Height = 20 };
                            Canvas.SetLeft(rightHand, variabPoint.X);
                            Canvas.SetTop(rightHand, variabPoint.Y);
                            pointCanvas.Children.Add(rightHand);
                            /*pointCanvas.Children.Add(
                                new Line() { Stroke = Brushes.Green, X1 = centerPoint.X - radius, Y1 = centerPoint.Y, X2 = centerPoint.X + radius, Y2 = centerPoint.Y, StrokeThickness = 20 });*/
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

        private Point coordinateMap (ColorSpacePoint point)
        {
            return new Point(float.IsInfinity(point.X) ? 0 : point.X / colorFrameDesc.Width * canvas.ActualWidth, float.IsInfinity(point.Y) ? 0 : point.Y / colorFrameDesc.Height * canvas.ActualHeight);
        }

        /// <summary>
        /// 判斷變異點是否進入扇形範圍
        /// </summary>
        /// <param name="centerX">中心點X座標(CX)</param>
        /// <param name="centerY">中心點Y座標(CY)</param>
        /// <param name="locateX">定位點X座標(LX)</param>
        /// <param name="locateY">定位點Y座標(LY)</param>
        /// <param name="variabX">變異點X座標(VX)</param>
        /// <param name="variabY">變異點Y座標(VY)</param>
        /// <param name="radius">自定義半徑(R)</param>
        /// <param name="angle">自定義角度(A)</param>
        private Boolean IsPointInSector (
            Double centerX, Double centerY, Double locateX, Double locateY,
            Double variabX, Double variabY, Double radius, Double angle)
        {
            //取得定位點V至中心點C之距離 VCD = V - C
            Double vcdX = variabX - centerX, vcdY = variabY - centerY;
            //取得VCD之絕對值 |VCD| = ( vcdX ^ 2 + vcdY ^ 2  ) ^ 0.5
            Double vcdABS = Math.Sqrt(vcdX * vcdX + vcdY * vcdY);
            //如果變異點至中心點的距離超過了半徑，表示變異點落在扇形以外，回傳false，跳離運算提升效能
            if (vcdABS > radius)
                return false;
            //取得定位點L至中心點C之距離 LCD = L - C
            Double lcdX = locateX - centerX, lcdY = locateY - centerY;
            //取得LCD之絕對值 |LCD| = ( lcdX ^ 2 + lcdY ^ 2  ) ^ 0.5
            Double lcdABS = Math.Sqrt(lcdX * lcdX + lcdY * lcdY);
            //取得LCD之單位向量 _LCD =  LCD / |LCD|
            Double _lcdX = lcdX / lcdABS, _lcdY = lcdY / lcdABS;
            LCDX.Content = _lcdX;
            LCDY.Content = _lcdY;
            //取得VCD之單位向量 _VCD =  VCD / |VCD|
            Double _vcdX = vcdX / vcdABS, _vcdY = vcdY / vcdABS;
            VCDX.Content = _vcdX;
            VCDY.Content = _vcdY;
            //取得定位點與變異點之內積 DOT = _LCD ‧ _VCD
            Double dot = _lcdX * _vcdX + _lcdY * _vcdY;
            DOT.Content = dot;
            //根據DOT判斷琴鍵觸發
            pressKey(_vcdX, dot);
            //取得定位點與變異點之夾角角度 _angel = Acos( _LCD ‧ _VCD ) * ( 180 / π )
            Double _angle = Math.Acos(dot) * ( 180 / Math.PI );
            ANG.Content = angle;
            _ANG.Content = _angle;
            //如果夾角角度小於自定義角度，表示變異點落在扇形以內，回傳判斷結果，函式結束
            return ( _angle < angle );
        }

        private void pressKey (Double variabX, Double dot)
        {
            if (variabX > 0)
            {
                if (dot > 0.85 && dot < 0.95)
                {
                    ( (PianoKeyWPF)( PianoControl.cnvPiano.Children[17] ) ).PressPianoKey();
                    pianoKeys[4].Fill = Brushes.Yellow;
                    pianoKeys[5].Fill = Brushes.White;
                    pianoKeys[6].Fill = Brushes.White;
                    pianoKeys[7].Fill = Brushes.White;
                }
                else if (dot > 0.65 && dot < 0.85)
                {
                    ( (PianoKeyWPF)( PianoControl.cnvPiano.Children[19] ) ).PressPianoKey();
                    pianoKeys[4].Fill = Brushes.White;
                    pianoKeys[5].Fill = Brushes.Yellow;
                    pianoKeys[6].Fill = Brushes.White;
                    pianoKeys[7].Fill = Brushes.White;
                }
                else if (dot > 0.35 && dot < 0.65)
                {
                    ( (PianoKeyWPF)( PianoControl.cnvPiano.Children[21] ) ).PressPianoKey();
                    pianoKeys[4].Fill = Brushes.White;
                    pianoKeys[5].Fill = Brushes.White;
                    pianoKeys[6].Fill = Brushes.Yellow;
                    pianoKeys[7].Fill = Brushes.White;
                }
                else if (dot > 0 && dot < 0.35)
                {
                    ( (PianoKeyWPF)( PianoControl.cnvPiano.Children[23] ) ).PressPianoKey();
                    pianoKeys[4].Fill = Brushes.White;
                    pianoKeys[5].Fill = Brushes.White;
                    pianoKeys[6].Fill = Brushes.White;
                    pianoKeys[7].Fill = Brushes.Yellow;
                }
                pianoKeys[0].Fill = Brushes.White;
                pianoKeys[1].Fill = Brushes.White;
                pianoKeys[2].Fill = Brushes.White;
                pianoKeys[3].Fill = Brushes.White;
            }
            else
            {
                if (dot > 0.85 && dot < 0.95)
                {
                    ( (PianoKeyWPF)( PianoControl.cnvPiano.Children[16] ) ).PressPianoKey();
                    pianoKeys[0].Fill = Brushes.White;
                    pianoKeys[1].Fill = Brushes.White;
                    pianoKeys[2].Fill = Brushes.White;
                    pianoKeys[3].Fill = Brushes.Yellow;
                }
                else if (dot > 0.65 && dot < 0.85)
                {
                    ( (PianoKeyWPF)( PianoControl.cnvPiano.Children[14] ) ).PressPianoKey();
                    pianoKeys[0].Fill = Brushes.White;
                    pianoKeys[1].Fill = Brushes.White;
                    pianoKeys[2].Fill = Brushes.Yellow;
                    pianoKeys[3].Fill = Brushes.White;
                }
                else if (dot > 0.35 && dot < 0.65)
                {
                    ( (PianoKeyWPF)( PianoControl.cnvPiano.Children[12] ) ).PressPianoKey();
                    pianoKeys[0].Fill = Brushes.White;
                    pianoKeys[1].Fill = Brushes.Yellow;
                    pianoKeys[2].Fill = Brushes.White;
                    pianoKeys[3].Fill = Brushes.White;
                }
                else if (dot > 0 && dot < 0.35)
                {
                    ( (PianoKeyWPF)( PianoControl.cnvPiano.Children[11] ) ).PressPianoKey();
                    pianoKeys[0].Fill = Brushes.Yellow;
                    pianoKeys[1].Fill = Brushes.White;
                    pianoKeys[2].Fill = Brushes.White;
                    pianoKeys[3].Fill = Brushes.White;
                }
                pianoKeys[4].Fill = Brushes.White;
                pianoKeys[5].Fill = Brushes.White;
                pianoKeys[6].Fill = Brushes.White;
                pianoKeys[7].Fill = Brushes.White;
            }
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

            cursorEllipse.Fill = ( handType == HandType.LEFT ) ? Brushes.Green : Brushes.Yellow;
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