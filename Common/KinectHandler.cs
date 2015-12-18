using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using Microsoft.Kinect.Input;

namespace AirBand
{
    public class KinectHandler : IDisposable
    {
        public VM_Body TrackingBody = null;
        public event KinectInputEventHandler InputEvent;
        private readonly int bytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;
        private KinectSensor kinectSensor = null;
        private KinectCoreWindow kinectCoreWindow;
        private CoordinateMapper coordinateMapper = null;
        private MultiSourceFrameReader multiFrameSourceReader = null;
        private WriteableBitmap bitmap = null;
        private WriteableBitmap bodyIndexBitmap = null;
        private uint bitmapBackBufferSize = 0;
        private uint[] bodyIndexPixels = null;
        private FrameDescription bodyIndexFrameDescription = null;
        private DepthSpacePoint[] colorMappedToDepthPoints = null;
        private Body[] bodies;
        private static readonly uint[] BodyColor =
        {
            0x0000FF00, 0x00FF0000, 0xFFFF4000,
            0x40FFFF00, 0xFF40FF00, 0xFF808000
        };

        public KinectHandler()
        {
            kinectSensor = KinectSensor.GetDefault();
            kinectSensor.IsAvailableChanged += Sensor_IsAvailableChanged;
            coordinateMapper = kinectSensor.CoordinateMapper;
            var depthFrameDescription = kinectSensor.DepthFrameSource.FrameDescription;
            var depthWidth = depthFrameDescription.Width;
            var depthHeight = depthFrameDescription.Height;
            var colorFrameDescription = kinectSensor.ColorFrameSource.FrameDescription;
            var colorWidth = colorFrameDescription.Width;
            var colorHeight = colorFrameDescription.Height;
            colorMappedToDepthPoints = new DepthSpacePoint[colorWidth * colorHeight];
            bitmap = new WriteableBitmap(colorWidth, colorHeight, 96.0, 96.0, PixelFormats.Bgra32, null);
            bitmapBackBufferSize = (uint)((bitmap.BackBufferStride * (bitmap.PixelHeight - 1)) + (bitmap.PixelWidth * bytesPerPixel));
            bodyIndexFrameDescription = kinectSensor.BodyIndexFrameSource.FrameDescription;
            bodyIndexPixels = new uint[bodyIndexFrameDescription.Width * bodyIndexFrameDescription.Height];
            bodyIndexBitmap = new WriteableBitmap(bodyIndexFrameDescription.Width, bodyIndexFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
        }

        #region IDisposable Support
        private bool disposedValue = false; // 偵測多餘的呼叫

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (multiFrameSourceReader != null)
                    {
                        multiFrameSourceReader.Dispose();
                        multiFrameSourceReader = null;
                    }

                    if (kinectSensor != null)
                    {
                        kinectSensor.Close();
                        kinectSensor = null;
                    }

                    if (kinectCoreWindow != null)
                    {
                        kinectCoreWindow = null;
                    }
                }

                // TODO: 釋放 Unmanaged 資源 (Unmanaged 物件) 並覆寫下方的完成項。
                // TODO: 將大型欄位設為 null。
                disposedValue = true;
            }
        }

        ~KinectHandler()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        public void StartRead()
        {
            multiFrameSourceReader = kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Depth | FrameSourceTypes.Color | FrameSourceTypes.Body | FrameSourceTypes.BodyIndex);
            multiFrameSourceReader.MultiSourceFrameArrived += reader_MultiSourceFrameArrived;
            kinectCoreWindow = KinectCoreWindow.GetForCurrentThread();
            kinectCoreWindow.PointerMoved += kinectCoreWindow_PointerMoved;
            kinectSensor.Open();
            TrackingBody = new VM_Body(0);
        }

        public void StopRead()
        {
            multiFrameSourceReader.MultiSourceFrameArrived -= reader_MultiSourceFrameArrived;
            multiFrameSourceReader.Dispose();
            multiFrameSourceReader = null;
            kinectCoreWindow.PointerMoved -= kinectCoreWindow_PointerMoved;
            kinectCoreWindow = null;
            kinectSensor.Close();
            TrackingBody = null;
        }

        public ImageSource ImageSource
        {
            get
            {
                return bitmap;
            }
        }

        public ImageSource BodyIndexImageSource
        {
            get
            {
                return bodyIndexBitmap;
            }
        }

        private unsafe void ProcessBodyIndexFrameData(IntPtr bodyIndexFrameData, uint bodyIndexFrameDataSize)
        {
            var frameData = (byte*)bodyIndexFrameData;
            for (int i = 0; i < (int)bodyIndexFrameDataSize; ++i)
            {
                if (frameData[i] < BodyColor.Length)
                    bodyIndexPixels[i] = BodyColor[frameData[i]];
                else
                    bodyIndexPixels[i] = 0x00000000;
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

        private void reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var multiSourceFrame = e.FrameReference.AcquireFrame();
            if (multiSourceFrame == null)
                return;
            var depthWidth = 0;
            var depthHeight = 0;
            DepthFrame depthFrame = null;
            ColorFrame colorFrame = null;
            BodyFrame bodyFrame = null;
            BodyIndexFrame bodyIndexFrame = null;
            var isBitmapLocked = false;
            try
            {
                depthFrame = multiSourceFrame.DepthFrameReference.AcquireFrame();
                colorFrame = multiSourceFrame.ColorFrameReference.AcquireFrame();
                bodyFrame = multiSourceFrame.BodyFrameReference.AcquireFrame();
                bodyIndexFrame = multiSourceFrame.BodyIndexFrameReference.AcquireFrame();
                if ((depthFrame == null) || (colorFrame == null) || (bodyIndexFrame == null) || (bodyFrame == null))
                    return;

                var depthFrameDescription = depthFrame.FrameDescription;
                depthWidth = depthFrameDescription.Width;
                depthHeight = depthFrameDescription.Height;
                using (var depthFrameData = depthFrame.LockImageBuffer())
                {
                    coordinateMapper.MapColorFrameToDepthSpaceUsingIntPtr(
                        depthFrameData.UnderlyingBuffer,
                        depthFrameData.Size,
                        colorMappedToDepthPoints);
                }
                depthFrame.Dispose();
                depthFrame = null;
                bitmap.Lock();
                isBitmapLocked = true;

                bodies = new Body[bodyFrame.BodyFrameSource.BodyCount];
                bodyFrame.GetAndRefreshBodyData(bodies);
                foreach (var body in bodies)
                {
                    if (body.IsTracked)
                    {
                        KinectCoreWindow.SetKinectOnePersonManualEngagement(new BodyHandPair(body.TrackingId, HandType.RIGHT));
                        var headPoint = coordinateMap(body.Joints[JointType.Head].Position); //2維頭心點
                        var centerPoint = coordinateMap(body.Joints[JointType.SpineMid].Position); //2維中心點
                        var spinePoint = coordinateMap(body.Joints[JointType.SpineBase].Position); //2維下心點
                        var locatePoint = coordinateMap(body.Joints[JointType.Head].Position); //2維定位點
                        var leftVariabPoint = coordinateMap(body.Joints[JointType.HandLeft].Position); //2維左異點
                        var rightVariabPoint = coordinateMap(body.Joints[JointType.HandRight].Position); //2維右異點
                        var shouldPoint = coordinateMap(body.Joints[JointType.SpineShoulder].Position); //2維中肩點
                        var leftHandState = body.HandLeftState; //左手狀態
                        var leftHandConfidence = body.HandLeftConfidence; //左手狀態程度
                        var rightHandState = body.HandRightState; //右手狀態
                        var rightHandConfidence = body.HandRightConfidence; //右手狀態程度
                        //更新體感數據
                        TrackingBody.UpdateBodyData(
                            body.Joints[JointType.HandRight].Position.Y > body.Joints[JointType.SpineMid].Position.Y,
                            body.TrackingId,
                            headPoint,
                            centerPoint, spinePoint,
                            locatePoint, leftVariabPoint,
                            rightVariabPoint, shouldPoint,
                            leftHandState, leftHandConfidence,
                            rightHandState, rightHandConfidence);
                    }
                }

                using (var bodyIndexBuffer = bodyIndexFrame.LockImageBuffer())
                {
                    if (((bodyIndexFrameDescription.Width * bodyIndexFrameDescription.Height) == bodyIndexBuffer.Size) &&
                        (bodyIndexFrameDescription.Width == bodyIndexBitmap.PixelWidth) && (bodyIndexFrameDescription.Height == bodyIndexBitmap.PixelHeight))
                    {
                        ProcessBodyIndexFrameData(bodyIndexBuffer.UnderlyingBuffer, bodyIndexBuffer.Size);
                        RenderBodyIndexPixels();
                    }
                }

                if (!Switcher.VM_EnvironmentVariables.BackgroundRemoval)
                {
                    var colorFrameDescription = colorFrame.FrameDescription;
                    using (var colorBuffer = colorFrame.LockRawImageBuffer())
                    {
                        if ((colorFrameDescription.Width == bitmap.PixelWidth) && (colorFrameDescription.Height == bitmap.PixelHeight))
                        {
                            colorFrame.CopyConvertedFrameDataToIntPtr(
                                bitmap.BackBuffer,
                                (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
                                ColorImageFormat.Bgra);
                            bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                        }
                    }
                    colorFrame.Dispose();
                    colorFrame = null;
                }
                else
                {
                    colorFrame.CopyConvertedFrameDataToIntPtr(bitmap.BackBuffer, bitmapBackBufferSize, ColorImageFormat.Bgra);
                    colorFrame.Dispose();
                    colorFrame = null;
                    using (var bodyIndexData = bodyIndexFrame.LockImageBuffer())
                        unsafe
                        {
                            var bodyIndexDataPointer = (byte*)bodyIndexData.UnderlyingBuffer;
                            var colorMappedToDepthPointCount = colorMappedToDepthPoints.Length;
                            fixed (DepthSpacePoint* colorMappedToDepthPointsPointer = colorMappedToDepthPoints)
                            {
                                uint* bitmapPixelsPointer = (uint*)bitmap.BackBuffer;

                                for (int colorIndex = 0; colorIndex < colorMappedToDepthPointCount; ++colorIndex)
                                {
                                    var colorMappedToDepthX = colorMappedToDepthPointsPointer[colorIndex].X;
                                    var colorMappedToDepthY = colorMappedToDepthPointsPointer[colorIndex].Y;
                                    if (!float.IsNegativeInfinity(colorMappedToDepthX) && !float.IsNegativeInfinity(colorMappedToDepthY))
                                    {
                                        var depthX = (int)(colorMappedToDepthX + 0.5f);
                                        var depthY = (int)(colorMappedToDepthY + 0.5f);
                                        var depthIndex = (depthY * depthWidth) + depthX;
                                        if ((depthX >= 0) && (depthX < depthWidth) && (depthY >= 0) && (depthY < depthHeight) && bodyIndexDataPointer[depthIndex] != 0xff)
                                            continue;
                                    }
                                    bitmapPixelsPointer[colorIndex] = 0;
                                }
                            }
                            bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
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
                if (bodyFrame != null)
                    bodyFrame.Dispose();
                if (bodyIndexFrame != null)
                    bodyIndexFrame.Dispose();
            }
        }

        private Point coordinateMap(CameraSpacePoint position)
        {
            var point = kinectSensor.CoordinateMapper.MapCameraPointToColorSpace(position);
            return new Point(
                float.IsInfinity(point.X) ? 0 : point.X / 1920 * 1366,
                float.IsInfinity(point.Y) ? 0 : point.Y / 1080 * 768);
        }

        private void kinectCoreWindow_PointerMoved(object sender, KinectPointerEventArgs args)
        {
            if (args.CurrentPoint.Properties.HandType != HandType.RIGHT || TrackingBody == null || TrackingBody.TrackingId != args.CurrentPoint.Properties.BodyTrackingId)
                return;

            var pointer = args.CurrentPoint;
            if (pointer.Properties.IsEngaged)
            {
                var pos = new Point(pointer.Position.X * 1366 - 25, pointer.Position.Y * 768 - 25);
                var state = (TrackingBody.RightHandState == HandState.Lasso) ? InputState.Open : InputState.Close;
                InputEvent(new KinectInputArgs(pos, state, TrackingBody.IsValid));
            }
        }

        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            //this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
            //                                                : Properties.Resources.SensorNotAvailableStatusText;
        }
    }
}