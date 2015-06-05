using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using KinectAirBand.Controls;
using Microsoft.Kinect;

namespace KinectAirBand
{
    public class BodyViewModel
    {
        public BodyViewModel (Int32 bodyIndex)
        {
            this.bodyIndex = bodyIndex;
        }

        private Int32 bodyIndex = 0;
        public Int32 BodyIndex
        {
            get
            {
                return this.bodyIndex;
            }
        }

        private UInt64 trackingId = 0;
        public UInt64 TrackingId
        {
            get
            {
                return this.trackingId;
            }
        }

        private Boolean isTracked = false;
        public Boolean IsTracked
        {
            get
            {
                return isTracked;
            }
        }

        private Point centerPoint;
        public Point CenterPoint
        {
            get
            {
                return centerPoint;
            }
        }

        private Point locatePoint;
        public Point LocatePoint
        {
            get
            {
                return locatePoint;
            }
        }

        private Point variabPoint;
        public Point VariabPoint
        {
            get
            {
                return variabPoint;
            }
        }

        private Point shouldPoint;
        public Point ShouldPoint
        {
            get
            {
                return shouldPoint;
            }
        }
        //自定義半徑 = 中心點距離 - 定位點距離 + 自定義常數
        public Double Radius
        {
            get
            {
                return Math.Abs(centerPoint.Y - locatePoint.Y + 100);
            }
        }

        private HandState leftHandState;
        public HandState LeftHandState
        {
            get
            {
                return leftHandState;
            }
        }

        private HandState rightHandState;
        public HandState RightHandState
        {
            get
            {
                return rightHandState;
            }
        }

        private UserControl instrument;
        public UserControl Instrument
        {
            get
            {
                return instrument;
            }
        }

        public void SetBodyData (Body body)
        {
            isTracked = body.IsTracked;
            trackingId = body.TrackingId;
        }

        public void UpdateBodyData (Point centerPoint, Point locatePoint, Point variabPoint, Point shouldPoint, HandState leftHandState, HandState rightHandState)
        {
            this.centerPoint = centerPoint;
            this.locatePoint = locatePoint;
            this.variabPoint = variabPoint;
            this.shouldPoint = shouldPoint;
            this.leftHandState = leftHandState;
            this.rightHandState = rightHandState;
        }

        public void SetInstrument (UserControl instrument)
        {
            this.instrument = instrument;
        }

        public void UpdateInstrument ()
        {
            Dictionary<Type, Action> @switch = new Dictionary<Type, Action>
            {
                { typeof(PianoControl), () => ((PianoControl)instrument).UpdatePianoKeys(this)}
            };
            @switch[instrument.GetType()]();
        }

        public void ClearInstrument (Grid canvas)
        {
            canvas.Children.Remove(this.Instrument);
            this.instrument = null;
        }
    }
}