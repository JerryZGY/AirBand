using System.Windows;
using System.Windows.Controls;

namespace AirBand.Controls
{
    public class Ctrl_HexagonCanvas : Canvas
    {
        /*  HexagonCanvas Map
                 (P0)       >R0
            (P5)      (P1)  >R1


            (P4)      (P2)  >R2
                 (P3)       >R3
             ||   ||   ||
             C0   C1   C2
        */
        private struct Hexagon
        {
            public static double Width = 1366, Height = 768;        //六角板寬高
            private static double uW = Width / 3, uH = Height / 5;  //UnitWidth: 六角板單位寬; UnitHeight: 六角板單位高
            private static double cW = 128, cH = 128;               //ChildWidth: 元素寬度; ChildHeight: 元素高度
            public static double C0 = uW - cW;                      //Column0 欄0位置
            public static double C1 = (3 * uW - cW) / 2;            //Column1 欄1位置
            public static double C2 = 2 * uW;                       //Column2 欄2位置
            public static double R0 = (uH - cH) / 2;                //Row0 列0位置
            public static double R1 = R0 * 13;                      //Row1 列1位置 = (3 * uH - cH) / 2
            public static double R2 = R0 * 37;                      //Row2 列2位置 = (7 * uH - cH) / 2
            public static double R3 = R0 * 49;                      //Row3 列3位置 = (9 * uH - cH) / 2
        }

        private static Point[] HexagonPoint =
        {
            new Point(Hexagon.C1, Hexagon.R0), new Point(Hexagon.C2, Hexagon.R1), new Point(Hexagon.C2, Hexagon.R2),
            new Point(Hexagon.C1, Hexagon.R3), new Point(Hexagon.C0, Hexagon.R2), new Point(Hexagon.C0, Hexagon.R1)
        };

        public Ctrl_HexagonCanvas()
        {
            Loaded += (s, e) =>
            {
                Width = Hexagon.Width;
                Height = Hexagon.Height;
                RenderTransformOrigin = new Point(.5, .5);
                if (InternalChildren.Count > 0 && InternalChildren.Count < 7)
                    for (int i = 0; i < InternalChildren.Count; i++)
                        InitializeProperty();
            };
        }

        public void InitializeProperty()
        {
            for (int i = 0; i < InternalChildren.Count; i++)
            {
                SetLeft(InternalChildren[i], HexagonPoint[i].X);
                SetTop(InternalChildren[i], HexagonPoint[i].Y);
            }
        }
    }
}