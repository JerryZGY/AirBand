using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
namespace KinectAirBand.Pages
{
    public partial class BRPlaying : UserControl, IDisposable
    {
        private Boolean disposed = false;
        private Boolean isLoaded = false;
        private KinectSensor sensor;
        private UInt32 bitmapBackBufferSize;
        private WriteableBitmap bitmap = null;
        private MultiSourceFrameReader reader;
        private FrameDescription colorFrameDesc;
        private DepthSpacePoint[] colorMappedToDepthPoints = new DepthSpacePoint[1920 * 1080];

        public BRPlaying (EnvironmentVariablesViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }

        #region IDisposable

        ~BRPlaying ()
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
                reader.MultiSourceFrameArrived -= reader_MultiSourceFrameArrived;
                if (disposing && isLoaded)
                {
                    reader.Dispose();
                }
                sensor = null;
                bitmapBackBufferSize = 0;
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
            sensor = KinectSensor.GetDefault();
            sensor.Open();
            bitmap = new WriteableBitmap(1920, 1080, 96.0, 96.0, PixelFormats.Bgra32, null);
            reader = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.BodyIndex);
            Image_Background.Visibility = System.Windows.Visibility.Visible;
            bitmapBackBufferSize = (uint)( ( bitmap.BackBufferStride * ( bitmap.PixelHeight - 1 ) ) + ( bitmap.PixelWidth * 4 ) );
            reader.MultiSourceFrameArrived += reader_MultiSourceFrameArrived;
            colorFrameDesc = sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            Image_BackgroundRemoval.Source = bitmap;
            StoryboardHandler.InitHitStoryBoard(this, "EnterStoryboard");

            disposed = false;
            isLoaded = true;
        }

        private void reader_MultiSourceFrameArrived (object sender, MultiSourceFrameArrivedEventArgs e)
        {
            MultiSourceFrame multiSourceFrame = e.FrameReference.AcquireFrame();
            DepthFrame depthFrame = null;
            ColorFrame colorFrame = null;
            BodyIndexFrame bodyIndexFrame = null;
            bool isBitmapLocked = false;
            if (multiSourceFrame == null)
                return;
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

        private void Button_Click (object sender, RoutedEventArgs e)
        {
            Dispose(true);
            StoryboardHandler.InitHitStoryBoard(this, "ExitStoryboard", () => Switcher.Switch("MainMenu"));
        }
    }
}