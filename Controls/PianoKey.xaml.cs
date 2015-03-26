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

namespace KinectAirBand.Controls
{
    /// <summary>
    /// PianoKey.xaml 的互動邏輯
    /// </summary>
    public partial class PianoKey : UserControl
    {
        private PianoControl.KeyType keyType = PianoControl.KeyType.White;
        private bool on = false;
        private LinearGradientBrush whiteKeyOnBrush;
        private LinearGradientBrush blackKeyOnBrush;
        private SolidColorBrush whiteKeyOffBrush = new SolidColorBrush(Colors.White);
        private int noteID = 60;
        public PianoControl.KeyType KeyType { get; set; }

        public PianoKey (PianoControl.KeyType keyType)
        {
            this.keyType = keyType;
            whiteKeyOnBrush = new LinearGradientBrush();
            whiteKeyOnBrush.GradientStops.Add(new GradientStop(Colors.White, 0.0));
            whiteKeyOnBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x20, 0x20, 0x20), 1.0));
            blackKeyOnBrush = new LinearGradientBrush();
            blackKeyOnBrush.GradientStops.Add(new GradientStop(Colors.LightGray, 0.0));
            blackKeyOnBrush.GradientStops.Add(new GradientStop(Colors.Black, 1.0));
            InitializeComponent();
            
        }

        public void PressPianoKey()
        {
            brdInner.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                new Action(
                    delegate()
                    {
                        if (keyType == PianoControl.KeyType.White)
                            brdInner.Background = whiteKeyOnBrush;
                        else
                            brdInner.Background = blackKeyOnBrush;
                    }
                )
            );
            on = true;
        }

        public void ReleasePianoKey()
        {
            brdInner.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                new Action(
                    delegate()
                    {
                        brdInner.Background = whiteKeyOffBrush;
                    }
                )
            );
            on = false;
        }

        public int NoteID
        {
            get
            {
                return noteID;
            }
            set
            {
                if (value < 0 || value > ShortMessage.DataMaxValue)
                {
                    throw new ArgumentOutOfRangeException("NoteID", noteID, "Note ID out of range.");
                }
                noteID = value;
                lbl.Content = noteID.ToString();
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
                if (keyType == PianoControl.KeyType.White)
                    brush = whiteKeyOnBrush;
                else
                    brush = blackKeyOnBrush;
                brdInner.Background = brush;
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
                brdInner.Background = whiteKeyOffBrush;
            }
        }

    }
}