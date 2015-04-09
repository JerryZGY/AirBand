using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace KinectAirBand
{
    class StateView : INotifyPropertyChanged
    {
        private Int32 bodyIndex = 0;
        private Boolean isTracked = false;
        private String rightHandX;
        private String rightHandY;
        private String rightHandZ;
        private String leftHandX;
        private String leftHandY;
        private String leftHandZ;

        public StateView (int bodyIndex, bool isTracked)
        {
            this.BodyIndex = bodyIndex;
            this.IsTracked = isTracked;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Int32 BodyIndex
        {
            get
            {
                return this.bodyIndex;
            }

            private set
            {
                if (this.bodyIndex != value)
                {
                    this.bodyIndex = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public Boolean IsTracked
        {
            get
            {
                return this.isTracked;
            }

            private set
            {
                if (this.IsTracked != value)
                {
                    this.isTracked = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public String RightHandX
        {
            get
            {
                return this.rightHandX;
            }

            private set
            {
                if (this.rightHandX != value)
                {
                    this.rightHandX = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public String RightHandY
        {
            get
            {
                return this.rightHandY;
            }

            private set
            {
                if (this.rightHandY != value)
                {
                    this.rightHandY = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public String RightHandZ
        {
            get
            {
                return this.rightHandZ;
            }

            private set
            {
                if (this.rightHandZ != value)
                {
                    this.rightHandZ = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public String LeftHandX
        {
            get
            {
                return this.leftHandX;
            }

            private set
            {
                if (this.leftHandX != value)
                {
                    this.leftHandX = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public String LeftHandY
        {
            get
            {
                return this.leftHandY;
            }

            private set
            {
                if (this.leftHandY != value)
                {
                    this.leftHandY = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public String LeftHandZ
        {
            get
            {
                return this.leftHandZ;
            }

            private set
            {
                if (this.leftHandZ != value)
                {
                    this.leftHandZ = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public void UpdateStateResult (Body body)
        {
            RightHandX = body.Joints[JointType.HandRight].Position.X.ToString("#0.00");
            RightHandY = body.Joints[JointType.HandTipRight].Position.Y.ToString("#0.00");
            RightHandZ = body.Joints[JointType.HandRight].Position.Z.ToString("#0.00");
            LeftHandX = body.Joints[JointType.HandLeft].Position.X.ToString("#0.00");
            LeftHandY = body.Joints[JointType.HandLeft].Position.Y.ToString("#0.00");
            LeftHandZ = body.Joints[JointType.HandLeft].Position.Z.ToString("#0.00");
        }

        private void NotifyPropertyChanged ([CallerMemberName] string propertyName = "")
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
