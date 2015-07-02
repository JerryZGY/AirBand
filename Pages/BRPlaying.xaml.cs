using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
namespace AirBand.Pages
{
    public partial class BRPlaying : UserControl
    {
        private readonly int bytesPerPixel = ( PixelFormats.Bgr32.BitsPerPixel + 7 ) / 8;
        private KinectSensor sensor = null;
        private CoordinateMapper coordinateMapper = null;
        private MultiSourceFrameReader reader = null;
        private WriteableBitmap bitmap = null;
        private uint bitmapBackBufferSize = 0;
        private DepthSpacePoint[] colorMappedToDepthPoints = null;

        public BRPlaying (EnvironmentVariablesViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }

        private void BRPlaying_Loaded (object sender, RoutedEventArgs e)
        {
            Grid_Main.Opacity = 0;

            sensor = KinectSensor.GetDefault();

            reader = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Depth | FrameSourceTypes.Color | FrameSourceTypes.BodyIndex);

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

            bitmapBackBufferSize =
                (uint)( ( bitmap.BackBufferStride * ( bitmap.PixelHeight - 1 ) ) + ( bitmap.PixelWidth * bytesPerPixel ) );

            Image_BackgroundRemoval.Source = bitmap;

            sensor.Open();

            StoryboardHandler.InitHitStoryBoard(this, "EnterStoryboard");
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
                    this.bitmap.Unlock();
                if (depthFrame != null)
                    depthFrame.Dispose();
                if (colorFrame != null)
                    colorFrame.Dispose();
                if (bodyIndexFrame != null)
                    bodyIndexFrame.Dispose();
            }
        }

        private void Button_Click (object sender, RoutedEventArgs e)
        {
            StoryboardHandler.InitHitStoryBoard(this, "ExitStoryboard", () => Switcher.Switch("MainMenu"));
        }

        
    }
}