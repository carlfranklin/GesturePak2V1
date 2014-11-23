using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using System.Windows.Media;
using System.Windows;

namespace GesturePak
{
    public class DrawingFrameEventArgs : EventArgs
    {
        public GesturePak.Frame Frame { get; set; }
        public Dictionary<JointType, Point> JointPoints { get; set; }
        public DrawingContext dc { get; set; }

        public DrawingFrameEventArgs(GesturePak.Frame Frame, Dictionary<JointType, Point> JointPoints, DrawingContext dc)
        {
            this.Frame = Frame;
            this.JointPoints = JointPoints;
            this.dc = dc;
        }
    }
}
