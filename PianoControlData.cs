using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Microsoft.Kinect.Input;

namespace KinectAirBand
{
    class PianoControlData
    {
        public PointF pointerPosition;
        private Body body;
        private Int32 index = 0;
        private UInt64 trackingid;

        public PianoControlData (Int32 index)
        {
            this.Index = index;
        }

        public Int32 Index
        {
            get
            {
                return this.index;
            }

            private set
            {
                if (this.index != value)
                {
                    this.index = value;
                }
            }
        }

        public UInt64 TrackingId
        {
            get
            {
                return this.trackingid;
            }

            private set
            {
                if (this.trackingid != value)
                {
                    this.trackingid = value;
                }
            }
        }

        public Boolean IsLeftHandLasso
        {
            get
            {
                return this.body.HandLeftState == HandState.Lasso;
            }
        }

        public Boolean IsRightHandLasso
        {
            get
            {
                return this.body.HandRightState == HandState.Lasso;
            }
        }

        public void GetPointerPosition (KinectPointerPoint point)
        {
            if (point.Properties.BodyTrackingId == trackingid)
            {
                pointerPosition = point.Position;
            }
        }

        public void UpdateBodyData (Body body)
        {
            this.body = body;
            this.trackingid = body.TrackingId;
        }
    }
}
