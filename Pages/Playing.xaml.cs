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
using Sanford.Multimedia.Midi;

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
        public ImageSource PointImageSource
        {
            get
            {
                return _pointWriteableBitmap;
            }
        }
        private KinectSensor _sensor;
        private MultiSourceFrameReader _reader;
        private Body[] _bodies;
        private List<StateView> _stateViewList = null;
        private DrawingGroup drawingGroup;
        private int displayWidth;
        private int displayHeight;
        Grid rootGrid;
        Image bodyImage;
        private RenderTargetBitmap _bodySourceRTB;
        private RenderTargetBitmap _pointSourceRTB;
        private byte[] bodyBytespixels = null;
        private readonly int bytesPerPixel = ( PixelFormats.Bgr32.BitsPerPixel + 7 ) / 8;
        private readonly WriteableBitmap _bodyWriteableBitmap;
        private readonly WriteableBitmap _pointWriteableBitmap;
        Line test = new Line() { X1 = 0, Y1 = 0, X2 = 0, Y2 = 344, StrokeThickness = 20, Stroke = Brushes.Red };
        private OutputDevice outDevice;
        private int pressedKey = 0;

        public Playing ()
        {
            _sensor = KinectSensor.GetDefault();
            _sensor.Open();
            _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Body);
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
            _pointSourceRTB = new RenderTargetBitmap(displayWidth, displayHeight, 96.0, 96.0, PixelFormats.Pbgra32);
            rootGrid = new Grid();
            _bodyWriteableBitmap = BitmapFactory.New(displayWidth, displayHeight);
             _pointWriteableBitmap = BitmapFactory.New(displayWidth, displayHeight);
             outDevice = new OutputDevice(0);
            InitializeComponent();
            cnv.Children.Add(test);
            Canvas.SetLeft(test, 688);
            DataContext = this;
            for (int i = 0; i < maxBodies; ++i)
            {
                _stateViewList.Add(new StateView(i, false));
                ContentControl contentControl = new ContentControl();
                contentControl.Content = _stateViewList[i];
                Grid.SetColumn(contentControl, i);
                Grid_State.Children.Add(contentControl);
            }
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

            /*using (DepthFrame depthFrame = reference.DepthFrameReference.AcquireFrame())
            {
                if (depthFrame != null)
                {
                    FrameDescription depthFrameDescription = depthFrame.FrameDescription;
                    using (KinectBuffer depthBuffer = depthFrame.LockImageBuffer())
                    {
                        _colorBitmap.Lock();
                        if (( depthFrameDescription.Width == this._colorBitmap.PixelWidth ) && ( depthFrameDescription.Height == _colorBitmap.PixelHeight ))
                        {
                            colorFrame.CopyConvertedFrameDataToIntPtr(this._colorBitmap.BackBuffer, (uint)( colorFrameDescription.Width * colorFrameDescription.Height * 4 ), ColorImageFormat.Bgra);
                            _colorBitmap.AddDirtyRect(new Int32Rect(0, 0, _colorBitmap.PixelWidth, _colorBitmap.PixelHeight));
                        }
                        _colorBitmap.Unlock();
                    }
                }
            }*/

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
                    //using (DrawingContext dc = this.drawingGroup.Open())
                    //{
                        //dc.DrawRectangle(Brushes.Transparent, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
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
                                foreach (PianoKey key in PianoControl.cnvPiano.Children)
                                {
                                    /*if (key.IsPianoKeyPressed && body.HandRightState != HandState.Lasso)
                                    {
                                        outDevice.Send(new ChannelMessage(ChannelCommand.NoteOff, 0, key.NoteID, 127));
                                        PianoControl.Send(new ChannelMessage(ChannelCommand.NoteOff, 0, key.NoteID, 127));
                                    }*/
                                    var toppos = (int)( jointPoints[JointType.HandTipRight].X );
                                    var keypos = (int)( Canvas.GetLeft(key));
                                    if (toppos - keypos <= 110 && toppos - keypos >= 0 &&
                                        body.Joints[JointType.HandTipRight].Position.Y >= 0 &&
                                        !key.releaseTimer.Enabled
                                       )
                                    {
                                        outDevice.Send(new ChannelMessage(ChannelCommand.NoteOn, 0, key.NoteID, 127));
                                        PianoControl.Send(new ChannelMessage(ChannelCommand.NoteOn, 0, key.NoteID, 127));
                                        
                                    }
                                    /*else if ((int)( jointPoints[JointType.HandRight].X ) - (int)( Canvas.GetLeft(key) + key.Width / 2 ) <= 30 &&
                                        (int)( jointPoints[JointType.HandRight].X ) - (int)( Canvas.GetLeft(key) + key.Width / 2 ) >= 0 &&
                                        body.HandRightState != HandState.Open
                                        )
                                    {
                                        outDevice.Send(new ChannelMessage(ChannelCommand.NoteOff, 0, pressedKey, 127));
                                        PianoControl.Send(new ChannelMessage(ChannelCommand.NoteOff, 0, pressedKey, 127));
                                        pressedKey = 0;
                                        outDevice.Send(new ChannelMessage(ChannelCommand.NoteOff, 0, key.NoteID, 127));
                                        PianoControl.Send(new ChannelMessage(ChannelCommand.NoteOff, 0, key.NoteID, 127));
                                    }*/
                                }
                                try
                                {
                                    Canvas.SetLeft(test, jointPoints[JointType.HandTipRight].X);

                                }
                                catch (Exception)
                                {

                                }
                                //Canvas.SetTop(test, jointPoints[JointType.HandRight].Y);
                                //dc.DrawEllipse(Brushes.Blue, null, jointPoints[JointType.HandTipRight], 30, 30);
                                //0 ~ 150
                                //PianoControl.Send(( (int)( joints[JointType.HandRight].Position.X * 100 ) ) + 80);
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
                        //testt = locatableCanvas;
                        _bodySourceRTB.Clear();
                        _bodySourceRTB.Render(PianoControl);
                        _bodySourceRTB.CopyPixels(this.bodyBytespixels, displayWidth * this.bytesPerPixel, 0);
                        _bodyWriteableBitmap.FromByteArray(this.bodyBytespixels);

                        _pointSourceRTB.Clear();
                        _pointSourceRTB.Render(cnv);
                        _pointSourceRTB.CopyPixels(this.bodyBytespixels, displayWidth * this.bytesPerPixel, 0);
                        _pointWriteableBitmap.FromByteArray(this.bodyBytespixels);
                        /*this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                        bodyImage = new Image { Source = new DrawingImage(drawingGroup), Width = this.displayWidth, Height = this.displayHeight };
                        rootGrid.Children.Clear();
                        rootGrid.Children.Add(bodyImage);
                        rootGrid.Measure(new Size(bodyImage.Width, bodyImage.Height));
                        rootGrid.Arrange(new Rect(0, 0, bodyImage.Width, bodyImage.Height));
                        _bodySourceRTB.Clear();
                        _bodySourceRTB.Render(testt);
                        _bodySourceRTB.CopyPixels(this.bodyBytespixels, displayWidth * this.bytesPerPixel, 0);
                        _bodyWriteableBitmap.FromByteArray(this.bodyBytespixels);*/
                    //}
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
