using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace KinectAirBand
{
    public class ToneTriggerHandler
    {
        private const Double range = 0.05;
        float HLX, HLY, HLZ, HRX, HRY, HRZ, WLX, WLY, WLZ, WRX, WRY, WRZ, ELX, ELY, ELZ, ERX, ERY, ERZ, SLX, SLY, SLZ, SRX, SRY, SRZ;

        public void UpdateBodyData (Body body)
        {
            this.HLX = body.Joints[JointType.HandLeft].Position.X;
            this.HLY = body.Joints[JointType.HandLeft].Position.Y;
            this.HLZ = body.Joints[JointType.HandLeft].Position.Z;
            this.HRX = body.Joints[JointType.HandRight].Position.X;
            this.HRY = body.Joints[JointType.HandRight].Position.Y;
            this.HRZ = body.Joints[JointType.HandRight].Position.Z;

            this.WLX = body.Joints[JointType.WristLeft].Position.X;
            this.WLY = body.Joints[JointType.WristLeft].Position.Y;
            this.WLZ = body.Joints[JointType.WristLeft].Position.Z;
            this.WRX = body.Joints[JointType.WristRight].Position.X;
            this.WRY = body.Joints[JointType.WristRight].Position.Y;
            this.WRZ = body.Joints[JointType.WristRight].Position.Z;

            this.ELX = body.Joints[JointType.ElbowLeft].Position.X;
            this.ELY = body.Joints[JointType.ElbowLeft].Position.Y;
            this.ELZ = body.Joints[JointType.ElbowLeft].Position.Z;
            this.ERX = body.Joints[JointType.ElbowRight].Position.X;
            this.ERY = body.Joints[JointType.ElbowRight].Position.Y;
            this.ERZ = body.Joints[JointType.ElbowRight].Position.Z;

            this.SLX = body.Joints[JointType.ShoulderLeft].Position.X;
            this.SLY = body.Joints[JointType.ShoulderLeft].Position.Y;
            this.SLZ = body.Joints[JointType.ShoulderLeft].Position.Z;
            this.SRX = body.Joints[JointType.ShoulderRight].Position.X;
            this.SRY = body.Joints[JointType.ShoulderRight].Position.Y;
            this.SRZ = body.Joints[JointType.ShoulderRight].Position.Z;
        }

        public Boolean CheckToneDoTrigger ()
        {
            var returnValue = false;
            if (( HLX - SRX ) >= -range && ( HLX - SRX) <= range)
            {
                if (( HLY - SRY ) >= -range && ( HLY - SRY ) <= range)
                {
                    if (( HLZ - SRZ ) >= -range && ( HLZ - SRZ ) <= range)
                        returnValue = true;
                }
            }
            return returnValue;
        }

        public Boolean CheckToneReTrigger ()
        {
            var returnValue = false;
            if (( HLX - ( SRX + ERX ) / 2 ) >= -range && ( HLX - ( SRX + ERX ) / 2 ) <= range)
            {
                if (( HLY - ( SRY + ERY) / 2 ) >= -range && ( HLY - ( SRY + ERY ) / 2 ) <= range)
                {
                    if (( HLZ - ( SRZ + ERZ ) / 2 ) >= -range && ( HLZ - ( SRZ + ERZ ) / 2 ) <= range)
                        returnValue = true;
                }
            }
            return returnValue;
        }

        public Boolean CheckToneMiTrigger ()
        {
            var returnValue = false;
            if (( HLX - ERX ) >= -range && ( HLX - ERX ) <= range)
            {
                if (( HLY - ERY ) >= -range && ( HLY - ERY ) <= range)
                {
                    if (( HLZ - ERZ ) >= -range && ( HLZ - ERZ ) <= range)
                        returnValue = true;
                }
            }
            return returnValue;
        }

        public Boolean CheckToneFaTrigger ()
        {
            var returnValue = false;
            if (( HLX - ( WRX + ERX ) / 2 ) >= -range && ( HLX - ( WRX + ERX ) / 2 ) <= range)
            {
                if (( HLY - ( WRY + ERY ) / 2 ) >= -range && ( HLY - ( WRY + ERY ) / 2 ) <= range)
                {
                    if (( HLZ - ( WRZ + ERZ ) / 2 ) >= -range && ( HLZ - ( WRZ + ERZ ) / 2 ) <= range)
                        returnValue = true;
                }
            }
            return returnValue;
        }

        public Boolean CheckToneSoTrigger ()
        {
            var returnValue = false;
            if (( HLX - WRX ) >= -range && ( HLX - WRX ) <= range)
            {
                if (( HLY - WRY ) >= -range && ( HLY - WRY ) <= range)
                {
                    if (( HLZ - WRZ ) >= -range && ( HLZ - WRZ ) <= range)
                        returnValue = true;
                }
            }
            return returnValue;
        }
    }
}
