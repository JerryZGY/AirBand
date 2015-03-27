using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Microsoft.Kinect;

namespace KinectAirBand
{
    public static class Extensions
    {
        private static void DrawBody (IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, DrawingContext drawingContext)
        {
            var trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
            var inferredJointBrush = Brushes.Yellow;
            DrawBone(joints, jointPoints, JointType.Head, JointType.Neck, drawingContext);
            DrawBone(joints, jointPoints, JointType.Neck, JointType.SpineShoulder, drawingContext);
            DrawBone(joints, jointPoints, JointType.SpineShoulder, JointType.SpineMid, drawingContext);
            DrawBone(joints, jointPoints, JointType.SpineMid, JointType.SpineBase, drawingContext);
            DrawBone(joints, jointPoints, JointType.SpineShoulder, JointType.ShoulderRight, drawingContext);
            DrawBone(joints, jointPoints, JointType.SpineShoulder, JointType.ShoulderLeft, drawingContext);
            DrawBone(joints, jointPoints, JointType.SpineBase, JointType.HipRight, drawingContext);
            DrawBone(joints, jointPoints, JointType.SpineBase, JointType.HipLeft, drawingContext);
            DrawBone(joints, jointPoints, JointType.ShoulderRight, JointType.ElbowRight, drawingContext);
            DrawBone(joints, jointPoints, JointType.ElbowRight, JointType.WristRight, drawingContext);
            DrawBone(joints, jointPoints, JointType.WristRight, JointType.HandRight, drawingContext);
            DrawBone(joints, jointPoints, JointType.HandRight, JointType.HandTipRight, drawingContext);
            DrawBone(joints, jointPoints, JointType.WristRight, JointType.ThumbRight, drawingContext);
            DrawBone(joints, jointPoints, JointType.ShoulderLeft, JointType.ElbowLeft, drawingContext);
            DrawBone(joints, jointPoints, JointType.ElbowLeft, JointType.WristLeft, drawingContext);
            DrawBone(joints, jointPoints, JointType.WristLeft, JointType.HandLeft, drawingContext);
            DrawBone(joints, jointPoints, JointType.HandLeft, JointType.HandTipLeft, drawingContext);
            DrawBone(joints, jointPoints, JointType.WristLeft, JointType.ThumbLeft, drawingContext);
            DrawBone(joints, jointPoints, JointType.HipRight, JointType.KneeRight, drawingContext);
            DrawBone(joints, jointPoints, JointType.KneeRight, JointType.AnkleRight, drawingContext);
            DrawBone(joints, jointPoints, JointType.AnkleRight, JointType.FootRight, drawingContext);
            DrawBone(joints, jointPoints, JointType.HipLeft, JointType.KneeLeft, drawingContext);
            DrawBone(joints, jointPoints, JointType.KneeLeft, JointType.AnkleLeft, drawingContext);
            DrawBone(joints, jointPoints, JointType.AnkleLeft, JointType.FootLeft, drawingContext);
            foreach (JointType jointType in joints.Keys)
            {
                Brush drawBrush = null;

                TrackingState trackingState = joints[jointType].TrackingState;

                if (trackingState == TrackingState.Tracked)
                {
                    drawBrush = trackedJointBrush;
                }
                else if (trackingState == TrackingState.Inferred)
                {
                    drawBrush = inferredJointBrush;
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, jointPoints[jointType], 3, 3);
                }
            }
        }

        private static void DrawBone (IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, JointType jointType0, JointType jointType1, DrawingContext drawingContext)
        {
            var trackedBonePen = new Pen(Brushes.Green, 6);
            var inferredBonePen = new Pen(Brushes.Gray, 1);
            Joint joint0 = joints[jointType0];
            Joint joint1 = joints[jointType1];

            if (joint0.TrackingState == TrackingState.NotTracked || joint1.TrackingState == TrackingState.NotTracked)
            {
                return;
            }

            if (joint0.TrackingState == TrackingState.Inferred && joint1.TrackingState == TrackingState.Inferred)
            {
                return;
            }

            Pen drawPen = inferredBonePen;
            if (( joint0.TrackingState == TrackingState.Tracked ) && ( joint1.TrackingState == TrackingState.Tracked ))
            {
                drawPen = trackedBonePen;
            }

            drawingContext.DrawLine(drawPen, jointPoints[jointType0], jointPoints[jointType1]);
        }

        private static void DrawHand (HandState handState, Point handPosition, DrawingContext drawingContext)
        {
            var handSize = 30;
            var handClosedBrush = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0));
            var handOpenBrush = new SolidColorBrush(Color.FromArgb(128, 0, 255, 0));
            var handLassoBrush = new SolidColorBrush(Color.FromArgb(128, 0, 0, 255));
            switch (handState)
            {
                case HandState.Closed:
                    drawingContext.DrawEllipse(handClosedBrush, null, handPosition, handSize, handSize);
                    break;

                case HandState.Open:
                    drawingContext.DrawEllipse(handOpenBrush, null, handPosition, handSize, handSize);
                    break;

                case HandState.Lasso:
                    drawingContext.DrawEllipse(handLassoBrush, null, handPosition, handSize, handSize);
                    break;
            }
        }

        public static Joint ScaleTo (this Joint joint, double width, double height, float skeletonMaxX, float skeletonMaxY)
        {
            joint.Position = new CameraSpacePoint
            {
                X = Scale(width, skeletonMaxX, joint.Position.X),
                Y = Scale(height, skeletonMaxY, -joint.Position.Y),
                Z = joint.Position.Z
            };

            return joint;
        }

        public static Joint ScaleTo (this Joint joint, double width, double height)
        {
            return ScaleTo(joint, width, height, 1.0f, 1.0f);
        }

        private static float Scale (double maxPixel, double maxSkeleton, float position)
        {
            float value = (float)( ( ( ( maxPixel / maxSkeleton ) / 2 ) * position ) + ( maxPixel / 2 ) );

            if (value > maxPixel)
            {
                return (float)maxPixel;
            }

            if (value < 0)
            {
                return 0;
            }

            return value;
        }
    }
}
