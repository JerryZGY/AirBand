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
        private readonly Brush handClosedBrush = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0));
        private readonly Brush handOpenBrush = new SolidColorBrush(Color.FromArgb(128, 0, 255, 0));
        private readonly Brush handLassoBrush = new SolidColorBrush(Color.FromArgb(128, 0, 0, 255));
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
        private readonly Brush inferredJointBrush = Brushes.Yellow;
        private const double JointThickness = 3;
        private const double HandSize = 30;
        private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);
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
            var test = PianoControl.cnvPiano.Children[3] as PianoKey;
            Vector offset = VisualTreeHelper.GetOffset(test);
            //var test = PianoControl.cnvPiano.Children[0];
            //var testt = Canvas.GetLeft(test);
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
                                dc.DrawEllipse(this.handClosedBrush, null, jointPoints[JointType.HandRight], HandSize, HandSize);
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

        public static bool CheckCollision (FrameworkElement a, FrameworkElement b)
        {
            Rect rect1 = new Rect((double)a.GetValue(Canvas.LeftProperty), (double)a.GetValue(Canvas.TopProperty), a.Width, a.Height);
            Rect rect2 = new Rect((double)b.GetValue(Canvas.LeftProperty), (double)b.GetValue(Canvas.TopProperty), b.Width, b.Height);
            return rect1.IntersectsWith(rect2);
        }

        private void DrawBody (IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, DrawingContext drawingContext)
        {
            // Draw the bones

            // Torso
            this.DrawBone(joints, jointPoints, JointType.Head, JointType.Neck, drawingContext);
            this.DrawBone(joints, jointPoints, JointType.Neck, JointType.SpineShoulder, drawingContext);
            this.DrawBone(joints, jointPoints, JointType.SpineShoulder, JointType.SpineMid, drawingContext);
            this.DrawBone(joints, jointPoints, JointType.SpineMid, JointType.SpineBase, drawingContext);
            this.DrawBone(joints, jointPoints, JointType.SpineShoulder, JointType.ShoulderRight, drawingContext);
            this.DrawBone(joints, jointPoints, JointType.SpineShoulder, JointType.ShoulderLeft, drawingContext);
            this.DrawBone(joints, jointPoints, JointType.SpineBase, JointType.HipRight, drawingContext);
            this.DrawBone(joints, jointPoints, JointType.SpineBase, JointType.HipLeft, drawingContext);

            // Right Arm    
            this.DrawBone(joints, jointPoints, JointType.ShoulderRight, JointType.ElbowRight, drawingContext);
            this.DrawBone(joints, jointPoints, JointType.ElbowRight, JointType.WristRight, drawingContext);
            this.DrawBone(joints, jointPoints, JointType.WristRight, JointType.HandRight, drawingContext);
            this.DrawBone(joints, jointPoints, JointType.HandRight, JointType.HandTipRight, drawingContext);
            this.DrawBone(joints, jointPoints, JointType.WristRight, JointType.ThumbRight, drawingContext);

            // Left Arm
            this.DrawBone(joints, jointPoints, JointType.ShoulderLeft, JointType.ElbowLeft, drawingContext);
            this.DrawBone(joints, jointPoints, JointType.ElbowLeft, JointType.WristLeft, drawingContext);
            this.DrawBone(joints, jointPoints, JointType.WristLeft, JointType.HandLeft, drawingContext);
            this.DrawBone(joints, jointPoints, JointType.HandLeft, JointType.HandTipLeft, drawingContext);
            this.DrawBone(joints, jointPoints, JointType.WristLeft, JointType.ThumbLeft, drawingContext);

            // Right Leg
            this.DrawBone(joints, jointPoints, JointType.HipRight, JointType.KneeRight, drawingContext);
            this.DrawBone(joints, jointPoints, JointType.KneeRight, JointType.AnkleRight, drawingContext);
            this.DrawBone(joints, jointPoints, JointType.AnkleRight, JointType.FootRight, drawingContext);

            // Left Leg
            this.DrawBone(joints, jointPoints, JointType.HipLeft, JointType.KneeLeft, drawingContext);
            this.DrawBone(joints, jointPoints, JointType.KneeLeft, JointType.AnkleLeft, drawingContext);
            this.DrawBone(joints, jointPoints, JointType.AnkleLeft, JointType.FootLeft, drawingContext);

            // Draw the joints
            foreach (JointType jointType in joints.Keys)
            {
                Brush drawBrush = null;

                TrackingState trackingState = joints[jointType].TrackingState;

                if (trackingState == TrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;
                }
                else if (trackingState == TrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, jointPoints[jointType], JointThickness, JointThickness);
                }
            }
        }

        private void DrawBone (IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, JointType jointType0, JointType jointType1, DrawingContext drawingContext)
        {
            Joint joint0 = joints[jointType0];
            Joint joint1 = joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == TrackingState.NotTracked ||
                joint1.TrackingState == TrackingState.NotTracked)
            {
                return;
            }

            // Don't draw if both points are inferred
            if (joint0.TrackingState == TrackingState.Inferred &&
                joint1.TrackingState == TrackingState.Inferred)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if (( joint0.TrackingState == TrackingState.Tracked ) && ( joint1.TrackingState == TrackingState.Tracked ))
            {
                drawPen = this.trackedBonePen;
            }

            drawingContext.DrawLine(drawPen, jointPoints[jointType0], jointPoints[jointType1]);
        }

        private void DrawHand (HandState handState, Point handPosition, DrawingContext drawingContext)
        {
            switch (handState)
            {
                case HandState.Closed:
                    drawingContext.DrawEllipse(this.handClosedBrush, null, handPosition, HandSize, HandSize);
                    break;

                case HandState.Open:
                    drawingContext.DrawEllipse(this.handOpenBrush, null, handPosition, HandSize, HandSize);
                    break;

                case HandState.Lasso:
                    drawingContext.DrawEllipse(this.handLassoBrush, null, handPosition, HandSize, HandSize);
                    break;
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

        private void ScalePosition (FrameworkElement element, Joint joint)
        {
            Joint scaledJoint = joint.ScaleTo(canvas.ActualWidth, canvas.ActualHeight);
            Canvas.SetLeft(element, scaledJoint.Position.X);
            Canvas.SetTop(element, scaledJoint.Position.Y);
        }
    }
}
