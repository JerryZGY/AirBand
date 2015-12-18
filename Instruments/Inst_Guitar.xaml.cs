using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using NAudio.Midi;

namespace AirBand.Instruments
{
    public partial class Inst_Guitar : UserControl
    {
        //弦箱數量
        private const int boxesCount = 6;
        //樂器音量
        private int volume = Switcher.VM_EnvironmentVariables.InstrumentsVolume;
        //體感數據
        private VM_Body body = Switcher.PageSwitcher.KinectHandler.TrackingBody;
        //MIDI設備
        private MidiOut outDevice = Switcher.PageSwitcher.MidiHandler.OutDevice;
        //吉他主體
        private Rectangle guitarChord;
        private List<Rectangle> guitarBoxes = new List<Rectangle>();
        private int[,] keysMap =
        {
            {84, 72, 60, 48, 36, 24},
            {86, 74, 62, 50, 38, 26},
            {88, 76, 64, 52, 40, 28},
            {89, 77, 65, 53, 41, 30},
            {91, 79, 67, 55, 43, 31},
            {93, 81, 69, 57, 45, 33},
            {95, 83, 71, 59, 47, 35},
        };
        private Rectangle hand;
        //吉他釋放計時器
        private System.Windows.Forms.Timer releaseTimer = new System.Windows.Forms.Timer() { Interval = Switcher.VM_EnvironmentVariables.GuitarSense };
        //吉他弦釋放計時器
        private System.Windows.Forms.Timer soundReleaseTimer = new System.Windows.Forms.Timer() { Interval = 3000 };

        public Inst_Guitar(Grid grid)
        {
            InitializeComponent();
            outDevice.Send(new PatchChangeEvent(0, 2, 25).GetAsShortMessage());
            createGuitar();
            Tag = grid;
            releaseTimer.Enabled = false;
            soundReleaseTimer.Enabled = false;
            hand = new Rectangle()
            {
                Fill = Brushes.Blue,
                Width = 20,
                Height = 20
            };
            Cnv_Main.Children.Add(hand);
        }

        private void createGuitar()
        {
            guitarChord = new Rectangle()
            {
                Fill = Brushes.White,
                Width = 20,
                Height = Math.Abs(body.SpinePoint.X - body.LeftVariabPoint.X),
                RenderTransformOrigin = new Point(0.5, 1),
                Opacity = 0.5
            };
            Canvas.SetBottom(guitarChord, 768 - (body.SpinePoint.Y + body.CenterPoint.Y) / 2);
            Canvas.SetLeft(guitarChord, body.SpinePoint.X - guitarChord.Width / 2);
            Canvas.SetZIndex(guitarChord, 0);
            Cnv_Main.Children.Add(guitarChord);

            for (int i = 0; i < boxesCount; i++)
            {
                Rectangle box = new Rectangle()
                {
                    Width = 100,
                    Height = 10,
                    Tag = false,
                    Opacity = 0.5
                };
                switch (i)
                {
                    case 0:
                        box.Fill = Brushes.Red;
                        break;
                    case 1:
                        box.Fill = Brushes.Orange;
                        break;
                    case 2:
                        box.Fill = Brushes.Yellow;
                        break;
                    case 3:
                        box.Fill = Brushes.Green;
                        break;
                    case 4:
                        box.Fill = Brushes.Blue;
                        break;
                    case 5:
                        box.Fill = Brushes.Indigo;
                        break;
                }
                guitarBoxes.Add(box);
                Canvas.SetTop(box, (body.SpinePoint.Y + body.CenterPoint.Y) / 4 + i * box.Height * 8);
                Canvas.SetLeft(box, body.SpinePoint.X - box.Width / 2 + 100);
                Canvas.SetZIndex(box, 1);
                Cnv_Main.Children.Add(box);
            }
        }

        /// <summary>
        /// 取得手腕角度
        /// </summary>
        /// <param name="centerX">中心點X座標(CX)</param>
        /// <param name="centerY">中心點Y座標(CY)</param>
        /// <param name="locateX">定位點X座標(LX)</param>
        /// <param name="locateY">定位點Y座標(LY)</param>
        /// <param name="variabX">變異點X座標(VX)</param>
        /// <param name="variabY">變異點Y座標(VY)</param>
        private double GetLeftHandAngle(
            double centerX, double centerY, double locateX, double locateY,
            double variabX, double variabY)
        {
            //取得定位點V至中心點C之距離 VCD = V - C
            double vcdX = variabX - centerX, vcdY = variabY - centerY;
            //如果變異點座標超過中心點，防止座標超出
            vcdX = (vcdX > 0) ? 0 : vcdX;
            //取得VCD之絕對值 |VCD| = ( vcdX ^ 2 + vcdY ^ 2  ) ^ 0.5
            double vcdABS = Math.Sqrt(vcdX * vcdX + vcdY * vcdY);
            //取得定位點L至中心點C之距離 LCD = L - C
            double lcdX = locateX - centerX, lcdY = locateY - centerY;
            //取得LCD之絕對值 |LCD| = ( lcdX ^ 2 + lcdY ^ 2  ) ^ 0.5
            double lcdABS = Math.Sqrt(lcdX * lcdX + lcdY * lcdY);
            //取得LCD之單位向量 _LCD =  LCD / |LCD|
            double _lcdX = lcdX / lcdABS, _lcdY = lcdY / lcdABS;
            //取得VCD之單位向量 _VCD =  VCD / |VCD|
            double _vcdX = vcdX / vcdABS, _vcdY = vcdY / vcdABS;
            //取得定位點與變異點之內積 DOT = _LCD ‧ _VCD
            double dot = _lcdX * _vcdX + _lcdY * _vcdY;
            //取得定位點與變異點之夾角角度 _angel = Acos( _LCD ‧ _VCD ) * ( 180 / π )
            double _angle = Math.Acos(dot) * (180 / Math.PI);
            //修正常數F(讓吉他更接近現實手腕位置)
            double fix = 0.75;
            //如果角度大於120，回傳120 * F；如果角度小於10，回傳10 / F；其餘回傳_angel *F
            return (_angle > 120) ? 120 * fix : (_angle < 10) ? 10 / fix : _angle * fix;
        }

        public void UpdateGuitar(VM_Body body)
        {
            var angle = GetLeftHandAngle(
                body.CenterPoint.X, body.CenterPoint.Y,
                body.LocatePoint.X, body.LocatePoint.Y,
                body.LeftVariabPoint.X, body.LeftVariabPoint.Y);
            guitarChord.RenderTransform = new RotateTransform(-90);
            guitarChord.Height = Math.Abs(body.SpinePoint.X - body.LeftVariabPoint.X);
            Canvas.SetBottom(guitarChord, 768 - (body.SpinePoint.Y + body.CenterPoint.Y) / 2);
            Canvas.SetLeft(guitarChord, body.SpinePoint.X - guitarChord.Width / 2);

            var chordPart = 60;
            int index1 = 0;

            if (guitarChord.Height <= chordPart)
            {
                guitarChord.Fill = Brushes.Red;
                index1 = 0;
            }
            else if (guitarChord.Height > chordPart * 1 && guitarChord.Height <= chordPart * 2)
            {
                guitarChord.Fill = Brushes.Orange;
                index1 = 1;
            }
            else if (guitarChord.Height > chordPart * 2 && guitarChord.Height <= chordPart * 3)
            {
                guitarChord.Fill = Brushes.Yellow;
                index1 = 2;
            }
            else if (guitarChord.Height > chordPart * 3 && guitarChord.Height <= chordPart * 4)
            {
                guitarChord.Fill = Brushes.Green;
                index1 = 3;
            }
            else if (guitarChord.Height > chordPart * 4 && guitarChord.Height <= chordPart * 5)
            {
                guitarChord.Fill = Brushes.Blue;
                index1 = 4;
            }
            else if (guitarChord.Height > chordPart * 5 && guitarChord.Height <= chordPart * 6)
            {
                guitarChord.Fill = Brushes.Indigo;
                index1 = 5;
            }
            else if (guitarChord.Height > chordPart * 6)
            {
                guitarChord.Fill = Brushes.Purple;
                index1 = 6;
            }

            foreach (var item in guitarBoxes.Select((value, i) => new { i, value }))
            {
                Rectangle box = item.value;
                box.Width = 100 + item.i * 30;
                //box.Height = length;
                Canvas.SetTop(box, (body.SpinePoint.Y + body.CenterPoint.Y) / 4 + item.i * box.Height * 8);
                Canvas.SetLeft(box, body.SpinePoint.X - box.Width / 2 + 100);
                pluckedGuitarBox(GetBounds(box, Cnv_Main), GetBounds(hand, Cnv_Main), index1, item.i);
            }

            Canvas.SetTop(hand, body.RightVariabPoint.Y);
            Canvas.SetLeft(hand, body.RightVariabPoint.X);
        }

        private Rect GetBounds(FrameworkElement of, FrameworkElement from)
        {
            GeneralTransform transform = of.TransformToVisual(from);
            return transform.TransformBounds(new Rect(0, 0, of.ActualWidth, of.ActualHeight));
        }

        private void pluckedGuitarBox(Rect box, Rect rightHand, int index1, int index2)
        {
            if ((bool)guitarBoxes[index2].Tag)
                return;

            if (box.IntersectsWith(rightHand))
            {
                outDevice.Send(new NoteOnEvent(0, 2, keysMap[index1, index2], volume, 0).GetAsShortMessage());
                releaseTimer.Enabled = true;
                releaseTimer.Tick += (s, e) =>
                {
                    releaseGuitarBox(index2);
                    releaseTimer.Enabled = false;
                };
                soundReleaseTimer.Enabled = true;
                soundReleaseTimer.Tick += (s, e) =>
                {
                    outDevice.Send(new NoteOnEvent(0, 2, keysMap[index1, index2], volume, 0).OffEvent.GetAsShortMessage());
                    soundReleaseTimer.Enabled = false;
                };
                guitarBoxes[index2].Tag = true;
            }
        }

        private void releaseGuitarBox(int index)
        {
            guitarBoxes[index].Tag = false;

        }
    }
}
