using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using AirBand.Instruments;
using Microsoft.Kinect;

namespace AirBand
{
    public class VM_Body
    {
        public VM_Body(int bodyIndex)
        {
            this.bodyIndex = bodyIndex;
        }

        private int bodyIndex = 0;
        public int BodyIndex
        {
            get
            {
                return bodyIndex;
            }
        }

        private ulong trackingId = 0;
        public ulong TrackingId
        {
            get
            {
                return trackingId;
            }
        }

        private bool isTracked = false;
        public bool IsTracked
        {
            get
            {
                return isTracked;
            }
        }

        private bool isValid = false;
        public bool IsValid
        {
            get
            {
                return isValid;
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
        public double Radius
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

        public void UpdateBodyData(
            bool isValid,
            ulong trackingId,
            Point headPoint,
            Point centerPoint, Point spinePoint,
            Point locatePoint, Point leftVariabPoint,
            Point rightVariabPoint, Point shouldPoint,
            HandState leftHandState, TrackingConfidence leftHandConfidence,
            HandState rightHandState, TrackingConfidence rightHandConfidence)
        {
            this.isValid = isValid;
            this.trackingId = trackingId;
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
            UpdateInstrument();
            UpdateMask();
        }

        public void SetInstrument(UserControl instrument)
        {
            this.instrument = instrument;
        }

        public void UpdateInstrument()
        {
            if (instrument == null)
                return;
            Dictionary<Type, Action> @switch = new Dictionary<Type, Action>
            {
                { typeof(Inst_Piano), () => ((Inst_Piano)instrument).UpdatePianoKeys(this)},
                { typeof(Inst_Guitar), () => ((Inst_Guitar)instrument).UpdateGuitar(this)},
                { typeof(Inst_Drum), () => ((Inst_Drum)instrument).UpdateDrum(this)},
            };
            @switch[instrument.GetType()]();
        }

        public void ClearInstrument()
        {
            instrument = null;
        }

        public void SetMask(Image mask)
        {
            this.mask = mask;
        }

        public void UpdateMask()
        {
            if (mask == null)
                return;
            mask.Stretch = System.Windows.Media.Stretch.Uniform;
            mask.Height = Math.Abs(headPoint.Y - spinePoint.Y) * 1.5;
            mask.Height /= 2;
            Canvas.SetTop(mask, headPoint.Y - mask.ActualHeight / 2);
            Canvas.SetLeft(mask, headPoint.X - mask.ActualWidth / 2);
        }

        public void ClearMask()
        {
            mask = null;
        }
    }
}