using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using AirBand.Controls;
using Microsoft.Kinect;

namespace AirBand
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

        private Point headPoint;
        public Point HeadPoint
        {
            get
            {
                return headPoint;
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

        private Point spinePoint;
        public Point SpinePoint
        {
            get
            {
                return spinePoint;
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

        private Point leftVariabPoint;
        public Point LeftVariabPoint
        {
            get
            {
                return leftVariabPoint;
            }
        }

        private Point rightVariabPoint;
        public Point RightVariabPoint
        {
            get
            {
                return rightVariabPoint;
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

        private TrackingConfidence leftHandConfidence;
        public TrackingConfidence LeftHandConfidence
        {
            get
            {
                return leftHandConfidence;
            }
        }

        private TrackingConfidence rightHandConfidence;
        public TrackingConfidence RightHandConfidence
        {
            get
            {
                return rightHandConfidence;
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

        private Image mask;
        public Image Mask
        {
            get
            {
                return mask;
            }
        }

        public void SetBodyData (Body body)
        {
            isTracked = body.IsTracked;
            trackingId = body.TrackingId;
        }

        public void UpdateBodyData (
            Point headPoint,
            Point centerPoint, Point spinePoint,
            Point locatePoint, Point leftVariabPoint,
            Point rightVariabPoint, Point shouldPoint,
            HandState leftHandState, TrackingConfidence leftHandConfidence,
            HandState rightHandState, TrackingConfidence rightHandConfidence)
        {
            this.headPoint = headPoint;
            this.centerPoint = centerPoint;
            this.spinePoint = spinePoint;
            this.locatePoint = locatePoint;
            this.leftVariabPoint = leftVariabPoint;
            this.rightVariabPoint = rightVariabPoint;
            this.shouldPoint = shouldPoint;
            this.leftHandState = leftHandState;
            this.leftHandConfidence = leftHandConfidence;
            this.rightHandState = rightHandState;
            this.rightHandConfidence = rightHandConfidence;
        }

        public void SetInstrument (UserControl instrument)
        {
            this.instrument = instrument;
        }

        public void UpdateInstrument ()
        {
            Dictionary<Type, Action> @switch = new Dictionary<Type, Action>
            {
                { typeof(PianoControl), () => ((PianoControl)instrument).UpdatePianoKeys(this)},
                { typeof(GuitarControl), () => ((GuitarControl)instrument).UpdateGuitar(this)}
            };
            @switch[instrument.GetType()]();
        }

        public void ClearInstrument ()
        {
            this.instrument = null;
        }

        public void SetMask (Image mask)
        {
            this.mask = mask;
        }

        public void UpdateMask ()
        {
            this.mask.Width /= 2;
            this.mask.Height /= 2;
            Canvas.SetTop(this.mask, headPoint.Y - this.mask.ActualHeight / 2);
            Canvas.SetLeft(this.mask, headPoint.X - this.mask.ActualWidth / 2);
        }

        public void ClearMask ()
        {
            this.mask = null;
        }
    }
}