using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using KinectAirBand;
using KinectAirBand.Controls;

namespace KinectAirBand.Pages
{
    public partial class Playing : UserControl, ISwitchable
    {
        public ImageSource ImageSource
        {
            get
            {
                return _colorBitmap;
            }
        }
        private WriteableBitmap _colorBitmap = null;
        public ImageSource BodyImageSource
        {
            get
            {
                return _bodyWriteableBitmap;
            }
        }
        private KinectSensor _sensor;
        private MultiSourceFrameReader _reader;
        private Body[] _bodies;
        private List<StateView> _stateViewList = null;
        private DrawingGroup drawingGroup;
        private int displayWidth;
        private int displayHeight;
        RenderTargetBitmap bmp;
        Grid rootGrid;
        Image img;
        Image bodyImage;
        private RenderTargetBitmap _bodySourceRTB;
        private byte[] bodyBytespixels = null;
        private readonly int bytesPerPixel = ( PixelFormats.Bgr32.BitsPerPixel + 7 ) / 8;
        private readonly WriteableBitmap _bodyWriteableBitmap;

        public Playing ()
        {
            _sensor = KinectSensor.GetDefault();
            _sensor.Open();
            _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
            _reader.MultiSourceFrameArrived += reader_MultiSourceFrameArrived;
            FrameDescription colorFrameDescription = _sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            _colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
            _stateViewList = new List<StateView>();
            int maxBodies = _sensor.BodyFrameSource.BodyCount;
            this.displayWidth = colorFrameDescription.Width;
            this.displayHeight = colorFrameDescription.Height;
            this.bodyBytespixels = new byte[displayWidth * displayHeight * this.bytesPerPixel];
            this.drawingGroup = new DrawingGroup();
            _bodySourceRTB = new RenderTargetBitmap(displayWidth, displayHeight, 96.0, 96.0, PixelFormats.Pbgra32);
            rootGrid = new Grid();
            _bodyWriteableBitmap = BitmapFactory.New(displayWidth, displayHeight);
            InitializeComponent();
            DataContext = this;
            for (int i = 0; i < maxBodies; ++i)
            {
                _stateViewList.Add(new StateView(i, false));
                ContentControl contentControl = new ContentControl();
                contentControl.Content = _stateViewList[i];
                Grid.SetColumn(contentControl, i);
                Grid_State.Children.Add(contentControl);
            }
            var test = PianoControl.cnvPiano.Children[10] as PianoKey;
            var testt = Canvas.GetLeft(test);
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
                        _colorBitmap.Lock();
                        if (( colorFrameDescription.Width == this._colorBitmap.PixelWidth ) && ( colorFrameDescription.Height == _colorBitmap.PixelHeight ))
                        {
                            colorFrame.CopyConvertedFrameDataToIntPtr(this._colorBitmap.BackBuffer, (uint)( colorFrameDescription.Width * colorFrameDescription.Height * 4 ), ColorImageFormat.Bgra);
                            _colorBitmap.AddDirtyRect(new Int32Rect(0, 0, _colorBitmap.PixelWidth, _colorBitmap.PixelHeight));
                        }
                        _colorBitmap.Unlock();
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
                    int maxBodies = _sensor.BodyFrameSource.BodyCount;
                    using (DrawingContext dc = this.drawingGroup.Open())
                    {
                        dc.DrawRectangle(Brushes.Transparent, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                        for (int i = 0; i < maxBodies; ++i)
                        {
                            Body body = _bodies[i];
                            _stateViewList[i].UpdateStateResult(body);
                            if (body.IsTracked)
                            {
                                IReadOnlyDictionary<JointType, Joint> joints = body.Joints;
                                Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();
                                foreach (JointType jointType in joints.Keys)
                                {
                                    ColorSpacePoint colorSpacePoint = _sensor.CoordinateMapper.MapCameraPointToColorSpace(joints[jointType].Position);
                                    jointPoints[jointType] = new Point(colorSpacePoint.X, colorSpacePoint.Y);
                                }
                                //0 ~ 150
                                PianoControl.Send(( (int)( joints[JointType.HandRight].Position.X * 100 ) ) + 80);
                                /*switch ((int)(joints[JointType.HandRight].Position.X * 10))
                                {
                                    case 0:
                                        PianoControl.Send()
                                    default:
                                        break;
                                }*/
                                //this.DrawBody(joints, jointPoints, dc);
                                //dc.DrawEllipse(this.handClosedBrush, null, jointPoints[JointType.HandLeft], HandSize, HandSize);
                                //dc.DrawEllipse(this.handClosedBrush, null, jointPoints[JointType.HandRight], HandSize, HandSize);
                                /*if(jointPoints[JointType.HandRight].X != null && jointPoints[JointType.HandRight].Y != null)
                                {
                                Canvas.SetLeft(leftEllipse, jointPoints[JointType.HandRight].X);
                                Canvas.SetTop(leftEllipse, jointPoints[JointType.HandRight].Y);
                                var test = PianoControl.cnvPiano.Children[1] as PianoKey;
                                if (CheckCollision(leftEllipse, test))
                                {
                                    Image_Test.Visibility = System.Windows.Visibility.Collapsed;
                                }*/
                                //}
                                //dc.DrawLine(trackedBonePen, jointPoints[JointType.HandLeft], jointPoints[JointType.HandRight]);
                                /*this.DrawHand(body.HandLeftState, jointPoints[JointType.HandLeft], dc);
                                this.DrawHand(body.HandRightState, jointPoints[JointType.HandRight], dc);*/
                            }
                        }
                        this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                        bodyImage = new Image { Source = new DrawingImage(drawingGroup), Width = this.displayWidth, Height = this.displayHeight };
                        rootGrid.Children.Clear();
                        rootGrid.Children.Add(bodyImage);
                        rootGrid.Measure(new Size(bodyImage.Width, bodyImage.Height));
                        rootGrid.Arrange(new Rect(0, 0, bodyImage.Width, bodyImage.Height));
                        _bodySourceRTB.Clear();
                        _bodySourceRTB.Render(rootGrid);
                        _bodySourceRTB.CopyPixels(this.bodyBytespixels, displayWidth * this.bytesPerPixel, 0);
                        _bodyWriteableBitmap.FromByteArray(this.bodyBytespixels);

                    }
                }
            }
        }

        #region ISwitchable Members

        public void UtilizeState (object state)
        {
            throw new NotImplementedException();
        }

        #endregion

        private void Button_Click (object sender, RoutedEventArgs e)
        {
            //Switcher.Switch(new Playing());
        }
    }
}
