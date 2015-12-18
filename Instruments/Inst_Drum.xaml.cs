using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using NAudio.Midi;

namespace AirBand.Instruments
{
    public partial class Inst_Drum : UserControl
    {
        //體感數據
        private VM_Body body = Switcher.PageSwitcher.KinectHandler.TrackingBody;
        //MIDI設備
        private MidiOut outDevice = Switcher.PageSwitcher.MidiHandler.OutDevice;
        //樂器音量
        private int volume = Switcher.VM_EnvironmentVariables.InstrumentsVolume;
        //左手點
        private Rectangle leftHand;
        //右手點
        private Rectangle rightHand;

        public Inst_Drum(Grid grid)
        {
            InitializeComponent();
            createDrum();
            Tag = grid;
            leftHand = new Rectangle()
            {
                Fill = Brushes.Transparent,
                Width = 25,
                Height = 25
            };
            Cnv_Main.Children.Add(leftHand);
            rightHand = new Rectangle()
            {
                Fill = Brushes.Transparent,
                Width = 25,
                Height = 25
            };
            Cnv_Main.Children.Add(rightHand);
        }

        private void createDrum()
        {
            RideCymbal.Tag = false;
            Tom1.Tag = false;
            Tom2.Tag = false;
            HiHat.Tag = false;
        }

        public void UpdateDrum(VM_Body body)
        {
            double length = body.Radius;
            Viewbox_Drum.Width = length * 2;
            Viewbox_Drum.Height = Viewbox_Drum.Width / 3.924;
            Canvas.SetTop(Viewbox_Drum, body.SpinePoint.Y - Viewbox_Drum.Height);
            Canvas.SetLeft(Viewbox_Drum, body.SpinePoint.X - Viewbox_Drum.Width / 2);
            Canvas.SetTop(Drum, body.SpinePoint.Y - Drum.Height);
            Canvas.SetLeft(Drum, body.SpinePoint.X - Drum.Width / 2);
            Canvas.SetTop(leftHand, body.LeftVariabPoint.Y - leftHand.Height / 2);
            Canvas.SetLeft(leftHand, body.LeftVariabPoint.X - leftHand.Width / 2);
            Canvas.SetTop(rightHand, body.RightVariabPoint.Y - rightHand.Height / 2);
            Canvas.SetLeft(rightHand, body.RightVariabPoint.X - rightHand.Width / 2);
            beatDrum(49);
            beatDrum(45);
            beatDrum(48);
            beatDrum(44);
        }

        private Rect GetBounds(FrameworkElement of, FrameworkElement from)
        {
            GeneralTransform transform = of.TransformToVisual(from);
            return transform.TransformBounds(new Rect(0, 0, of.ActualWidth, of.ActualHeight));
        }

        private void beatDrum(int drumID)
        {
            Rectangle drumType;
            Storyboard sb;
            switch (drumID)
            {
                case 49:
                    drumType = RideCymbal;
                    sb = (Storyboard)FindResource("sbRideCymbal");
                    break;
                case 45:
                    drumType = Tom1;
                    sb = (Storyboard)FindResource("sbTom1");
                    break;
                case 48:
                    drumType = Tom2;
                    sb = (Storyboard)FindResource("sbTom2");
                    break;
                default:
                    drumType = HiHat;
                    sb = (Storyboard)FindResource("sbHiHat");
                    break;
            }
            bool beated = (bool)drumType.Tag;

            if (GetBounds(drumType, Cnv_Main).IntersectsWith(GetBounds((drumID == 49 || drumID == 45) ? leftHand : rightHand, Cnv_Main)))
            {
                if (!beated)
                {
                    outDevice.Send(new NoteOnEvent(0, 10, drumID, volume, 0).GetAsShortMessage());
                    drumType.Tag = true;
                    sb.Begin();
                }
            }
            else
                drumType.Tag = false;
        }
    }
}