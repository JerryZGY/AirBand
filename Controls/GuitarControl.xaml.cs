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
using Sanford.Multimedia.Midi;

namespace KinectAirBand.Controls
{
    /// <summary>
    /// GuitarControl.xaml 的互動邏輯
    /// </summary>
    public partial class GuitarControl : UserControl
    {
        //弦箱數量
        private const Int32 boxesCount = 6;
        //體感數據
        private BodyViewModel body;
        //MIDI設備
        private OutputDevice outDevice;
        //吉他主體
        private Rectangle guitarChord;
        private List<Rectangle> guitarBoxes = new List<Rectangle>();
        private Int32[,] keysMap = 
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
        private System.Windows.Forms.Timer releaseTimer = new System.Windows.Forms.Timer() { Interval = 500 };

        public GuitarControl (OutputDevice outDevice, BodyViewModel body, Grid grid)
        {
            InitializeComponent();

            this.outDevice = outDevice;
            this.body = body;
            outDevice.Send(new ChannelMessage(ChannelCommand.ProgramChange, 1, 25, 0));

            createGuitar();
            this.Tag = grid;
            releaseTimer.Enabled = false;

            hand = new Rectangle()
            {
                Fill = Brushes.Blue,
                Width = 20,
                Height = 20
            };
            Canvas_Main.Children.Add(hand);
        }

        private void createGuitar ()
        {
            guitarChord = new Rectangle()
            {
                Fill = Brushes.White,
                Width = 20,
                Height = Math.Abs(body.SpinePoint.X - body.LeftVariabPoint.X),
                RenderTransformOrigin = new Point(0.5, 1)
            };
            Canvas.SetBottom(guitarChord, 768 - ( body.SpinePoint.Y + body.CenterPoint.Y ) / 2);
            Canvas.SetLeft(guitarChord, body.SpinePoint.X - guitarChord.Width / 2);
            Canvas.SetZIndex(guitarChord, 0);
            Canvas_Main.Children.Add(guitarChord);

            for (int i = 0; i < boxesCount; i++)
            {
                Rectangle box = new Rectangle()
                {
                    Width = 100,
                    Height = 10,
                    Tag = false
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
                Canvas.SetTop(box, ( body.SpinePoint.Y + body.CenterPoint.Y ) / 4 + i * box.Height * 8);
                Canvas.SetLeft(box, body.SpinePoint.X - box.Width / 2 + 100);
                Canvas.SetZIndex(box, 1);
                Canvas_Main.Children.Add(box);
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
        private Double GetLeftHandAngle (
            Double centerX, Double centerY, Double locateX, Double locateY,
            Double variabX, Double variabY)
        {
            //取得定位點V至中心點C之距離 VCD = V - C
            Double vcdX = variabX - centerX, vcdY = variabY - centerY;
            //如果變異點座標超過中心點，防止座標超出
            vcdX = ( vcdX > 0 ) ? 0 : vcdX;
            //取得VCD之絕對值 |VCD| = ( vcdX ^ 2 + vcdY ^ 2  ) ^ 0.5
            Double vcdABS = Math.Sqrt(vcdX * vcdX + vcdY * vcdY);
            //取得定位點L至中心點C之距離 LCD = L - C
            Double lcdX = locateX - centerX, lcdY = locateY - centerY;
            //取得LCD之絕對值 |LCD| = ( lcdX ^ 2 + lcdY ^ 2  ) ^ 0.5
            Double lcdABS = Math.Sqrt(lcdX * lcdX + lcdY * lcdY);
            //取得LCD之單位向量 _LCD =  LCD / |LCD|
            Double _lcdX = lcdX / lcdABS, _lcdY = lcdY / lcdABS;
            //取得VCD之單位向量 _VCD =  VCD / |VCD|
            Double _vcdX = vcdX / vcdABS, _vcdY = vcdY / vcdABS;
            //取得定位點與變異點之內積 DOT = _LCD ‧ _VCD
            Double dot = _lcdX * _vcdX + _lcdY * _vcdY;
            //取得定位點與變異點之夾角角度 _angel = Acos( _LCD ‧ _VCD ) * ( 180 / π )
            Double _angle = Math.Acos(dot) * ( 180 / Math.PI );
            //修正常數F(讓吉他更接近現實手腕位置)
            Double fix = 0.75;
            //如果角度大於120，回傳120 * F；如果角度小於10，回傳10 / F；其餘回傳_angel *F
            return ( _angle > 120 ) ? 120 * fix : ( _angle < 10 ) ? 10 / fix : _angle * fix;
        }

        public void UpdateGuitar (BodyViewModel body)
        {
            Double angle = GetLeftHandAngle(
                body.CenterPoint.X, body.CenterPoint.Y,
                body.LocatePoint.X, body.LocatePoint.Y,
                body.LeftVariabPoint.X, body.LeftVariabPoint.Y);
            guitarChord.RenderTransform = new RotateTransform(-90);
            guitarChord.Height = Math.Abs(body.SpinePoint.X - body.LeftVariabPoint.X);
            Canvas.SetBottom(guitarChord, 768 - ( body.SpinePoint.Y + body.CenterPoint.Y ) / 2);
            Canvas.SetLeft(guitarChord, body.SpinePoint.X - guitarChord.Width / 2);

            var chordPart = 60;
            Int32 index1 = 0;

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
                Canvas.SetTop(box, ( body.SpinePoint.Y + body.CenterPoint.Y ) / 4 + item.i * box.Height * 8);
                Canvas.SetLeft(box, body.SpinePoint.X - box.Width / 2 + 100);
                pluckedGuitarBox(GetBounds(box, Canvas_Main), GetBounds(hand, Canvas_Main), index1, item.i);
            }

            Canvas.SetTop(hand, body.RightVariabPoint.Y);
            Canvas.SetLeft(hand, body.RightVariabPoint.X);
        }

        private Rect GetBounds (FrameworkElement of, FrameworkElement from)
        {
            GeneralTransform transform = of.TransformToVisual(from);
            return transform.TransformBounds(new Rect(0, 0, of.ActualWidth, of.ActualHeight));
        }

        private void pluckedGuitarBox (Rect box, Rect rightHand, Int32 index1, Int32 index2)
        {
            if ((Boolean)guitarBoxes[index2].Tag)
                return;

            if (box.IntersectsWith(rightHand))
            {
                outDevice.Send(new ChannelMessage(ChannelCommand.NoteOn, 1, keysMap[index1, index2], 127));
                releaseTimer.Enabled = true;
                releaseTimer.Tick += (s, e) =>
                {
                    releaseGuitarBox(index2);
                    releaseTimer.Enabled = false;
                };
                guitarBoxes[index2].Tag = true;
            }
        }

        private void releaseGuitarBox (Int32 index)
        {
            guitarBoxes[index].Tag = false;
        }

        /*public void PressPianoKey (Int32 index)
        {
            if ((Boolean)pianoKeys[index].Tag)
                return;

            outDevice.Send(new ChannelMessage(ChannelCommand.NoteOn, 0, keysMap[index], 127));
            pianoKeys[index].Fill = Brushes.Yellow;
            releaseTimer.Enabled = true;
            releaseTimer.Tick += (s, e) =>
            {
                ReleasePianoKey(index);
                releaseTimer.Enabled = false;
            };
            pianoKeys[index].Tag = true;
        }

        public void ReleasePianoKey (Int32 index)
        {
            outDevice.Send(new ChannelMessage(ChannelCommand.NoteOn, 0, keysMap[index], 0));
            pianoKeys[index].Fill = Brushes.White;
            pianoKeys[index].Tag = false;
        }*/
    }
}
