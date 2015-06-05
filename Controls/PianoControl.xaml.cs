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
using Sanford.Multimedia.Midi.UI;

namespace KinectAirBand.Controls
{
    /// <summary>
    /// Piano.xaml 的互動邏輯
    /// </summary>
    public partial class PianoControl : UserControl
    {
        //自定義角度
        private const Int32 angle = 90;
        //琴鍵數量
        private const Int32 keysCount = 8;
        //控制項寬度
        private const Int32 controlWidth = 1366;
        //控制項高度
        private const Int32 controlHeight = 768;
        //初始旋轉角度
        private const Double initialValue = -90;
        //間隔旋轉角度
        private const Double intervalValue = 22.5;
        //體感數據
        private BodyViewModel body;
        //MIDI設備
        private OutputDevice outDevice;
        //琴鍵清單
        private List<Rectangle> pianoKeys = new List<Rectangle>();
        //琴鍵釋放計時器
        private System.Windows.Forms.Timer releaseTimer = new System.Windows.Forms.Timer() { Interval = 1000 };
        //琴鍵音調表
        public Int32[] keysMap = 
        {
            60, 62, 64, 65, 67, 69, 71, 72
        };

        public PianoControl (OutputDevice outDevice, BodyViewModel body)
        {
            InitializeComponent();

            this.outDevice = outDevice;
            this.body = body;

            createPianoKeys();

            releaseTimer.Enabled = false;
        }

        private void createPianoKeys ()
        {
            Double x = body.CenterPoint.X;
            Double y = body.ShouldPoint.Y;
            Double length = body.Radius;

            for (int i = 0; i < keysCount; i++)
            {
                Rectangle key = new Rectangle()
                {
                    Fill = Brushes.White,
                    Width = length / 5,
                    Height = length,
                    Tag = false
                };
                pianoKeys.Add(key);
                Canvas.SetTop(key, y - length * 0.8);
                if (i < 4)
                {
                    Canvas.SetLeft(key, x - length * 0.2);
                    key.RenderTransform = new RotateTransform(initialValue + intervalValue * i);
                    key.RenderTransformOrigin = new Point(1, 1);
                }
                else
                {
                    Canvas.SetLeft(key, x - length * 0.03);
                    key.RenderTransform = new RotateTransform(initialValue + intervalValue * ( i + 1 ));
                    key.RenderTransformOrigin = new Point(0, 1);
                }
                Canvas_Main.Children.Add(key);
            }
        }

        /// <summary>
        /// 判斷變異點是否進入扇形範圍
        /// </summary>
        /// <param name="centerX">中心點X座標(CX)</param>
        /// <param name="centerY">中心點Y座標(CY)</param>
        /// <param name="locateX">定位點X座標(LX)</param>
        /// <param name="locateY">定位點Y座標(LY)</param>
        /// <param name="variabX">變異點X座標(VX)</param>
        /// <param name="variabY">變異點Y座標(VY)</param>
        /// <param name="radius">自定義半徑(R)</param>
        /// <param name="angle">自定義角度(A)</param>
        private Boolean IsPointInSector (
            Double centerX, Double centerY, Double locateX, Double locateY,
            Double variabX, Double variabY, Double radius, Double angle)
        {
            //取得定位點V至中心點C之距離 VCD = V - C
            Double vcdX = variabX - centerX, vcdY = variabY - centerY;
            //取得VCD之絕對值 |VCD| = ( vcdX ^ 2 + vcdY ^ 2  ) ^ 0.5
            Double vcdABS = Math.Sqrt(vcdX * vcdX + vcdY * vcdY);
            //如果變異點至中心點的距離超過了半徑，表示變異點落在扇形以外，回傳false，跳離運算提升效能
            if (vcdABS > radius)
                return false;
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
            //根據DOT判斷琴鍵觸發
            pressKey(_vcdX, dot);
            //取得定位點與變異點之夾角角度 _angel = Acos( _LCD ‧ _VCD ) * ( 180 / π )
            Double _angle = Math.Acos(dot) * ( 180 / Math.PI );
            //如果夾角角度小於自定義角度，表示變異點落在扇形以內，回傳判斷結果，函式結束
            return ( _angle < angle );
        }

        private void pressKey (Double variabX, Double dot)
        {
            if (variabX > 0)
            {
                if (dot > 0.85 && dot < 0.95)
                {
                    PressPianoKey(4);
                }
                else if (dot > 0.65 && dot < 0.85)
                {
                    PressPianoKey(5);
                }
                else if (dot > 0.35 && dot < 0.65)
                {
                    PressPianoKey(6);
                }
                else if (dot > 0 && dot < 0.35)
                {
                    PressPianoKey(7);
                }
            }
            else
            {
                if (dot > 0.85 && dot < 0.95)
                {
                    PressPianoKey(3);
                }
                else if (dot > 0.65 && dot < 0.85)
                {
                    PressPianoKey(2);
                }
                else if (dot > 0.35 && dot < 0.65)
                {
                    PressPianoKey(1);
                }
                else if (dot > 0 && dot < 0.35)
                {
                    PressPianoKey(0);
                }
            }
        }

        public void UpdatePianoKeys (BodyViewModel body)
        {
            Double x = body.CenterPoint.X;
            Double y = body.ShouldPoint.Y;
            Double length = body.Radius;

            foreach (Rectangle key in pianoKeys)
            {
                key.Width = length / 5;
                key.Height = length;
                Canvas.SetTop(key, y - length * 0.8);
                Canvas.SetLeft(key, ( pianoKeys.IndexOf(key) < 4 ) ? x - length * 0.2 : x - length * 0.03);
            }

            IsPointInSector(
                body.CenterPoint.X, body.CenterPoint.Y,
                body.LocatePoint.X, body.LocatePoint.Y,
                body.VariabPoint.X, body.VariabPoint.Y,
                length, angle);
        }

        public void PressPianoKey (Int32 index)
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
        }
    }
}