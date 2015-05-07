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
using Sanford.Multimedia.Midi;
using Sanford.Multimedia.Midi.UI;

namespace KinectAirBand.Controls
{
    /// <summary>
    /// PianoKeyWPF.xaml 的互動邏輯
    /// </summary>
    public partial class PianoKeyWPF : UserControl
    {
        private PianoControlWPF.KeyType keyType = PianoControlWPF.KeyType.White;
        private PianoControlWPF owner;
        private bool on = false;
        private LinearGradientBrush whiteKeyOnBrush;
        private LinearGradientBrush blackKeyOnBrush;
        private SolidColorBrush whiteKeyOffBrush = new SolidColorBrush(Colors.White);
        private int noteID = 60;
        public System.Windows.Forms.Timer releaseTimer = new System.Windows.Forms.Timer() { Interval = 1000 };

        public PianoKeyWPF (PianoControlWPF owner, PianoControlWPF.KeyType keyType)
        {
            this.owner = owner;
            this.keyType = keyType;
            whiteKeyOnBrush = new LinearGradientBrush();
            whiteKeyOnBrush.GradientStops.Add(new GradientStop(Colors.White, 0.0));
            whiteKeyOnBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x20, 0x20, 0x20), 1.0));
            blackKeyOnBrush = new LinearGradientBrush();
            blackKeyOnBrush.GradientStops.Add(new GradientStop(Colors.LightGray, 0.0));
            blackKeyOnBrush.GradientStops.Add(new GradientStop(Colors.Black, 1.0));
            releaseTimer.Enabled = false;
            InitializeComponent();
        }

        public void PressPianoKey ()
        {
            
            this.Dispatcher.Invoke(
          System.Windows.Threading.DispatcherPriority.Normal,
            new Action(
                delegate()
                {
                    if (keyType == PianoControlWPF.KeyType.White)
                    {
                        this.Background = whiteKeyOnBrush;
                        releaseTimer.Enabled = true;
                        releaseTimer.Tick += (s, e) =>
                        {
                            this.Background = whiteKeyOffBrush;
                            ReleasePianoKey();
                            releaseTimer.Enabled = false;
                        };
                    }
                    else
                    {
                        this.Background = blackKeyOnBrush;
                        releaseTimer.Enabled = true;
                        releaseTimer.Tick += (s, e) =>
                        {
                            this.Background = whiteKeyOffBrush;
                            ReleasePianoKey();
                            releaseTimer.Enabled = false;
                        };
                    }
                }
            ));

            on = true;
            owner.OnPianoKeyDown(new PianoKeyEventArgs(noteID));
        }

        public void ReleasePianoKey ()
        {
            this.Dispatcher.Invoke(
          System.Windows.Threading.DispatcherPriority.Normal,
            new Action(
                delegate()
                {
                    this.Background = whiteKeyOffBrush;
                }
            ));

            on = false;
            owner.OnPianoKeyUp(new PianoKeyEventArgs(noteID));
        }

        public int NoteID
        {
            get
            {
                return noteID;
            }
            set
            {
                #region Require

                if (value < 0 || value > ShortMessage.DataMaxValue)
                {
                    throw new ArgumentOutOfRangeException("NoteID", noteID,
                        "Note ID out of range.");
                }

                #endregion

                noteID = value;
            }
        }

        public bool IsPianoKeyPressed
        {
            get
            {
                return on;
            }
        }

        public Color NoteOnColor
        {
            set
            {
                Brush brush = null;
                if (keyType == PianoControlWPF.KeyType.White)
                    brush = whiteKeyOnBrush;
                else
                    brush = blackKeyOnBrush;
                this.Background = brush;
            }
        }

        public Color NoteOffColor
        {
            get
            {
                return whiteKeyOffBrush.Color;
            }
            set
            {
                whiteKeyOffBrush.Color = value;
                this.Background = whiteKeyOffBrush;
            }
        }

        public PianoControlWPF.KeyType KeyType { get; set; }

        private void brdInner_PreviewMouseDown (object sender, MouseButtonEventArgs e)
        {
            PressPianoKey();
        }

        private void brdInner_PreviewMouseUp (object sender, MouseButtonEventArgs e)
        {
            ReleasePianoKey();

        }
    }
}
