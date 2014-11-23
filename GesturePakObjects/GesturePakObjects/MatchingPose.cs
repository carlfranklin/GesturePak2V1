using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GesturePak
{
    public class MatchingFrame
    {
        public Gesture Gesture { get; set; }
        public Frame Frame { get; set; }
        public Single Delta { get; set; }
        public DateTime TimeStamp { get; set; }

        public MatchingFrame(Gesture Gesture, Frame Frame, Single Delta)
        {
            this.Gesture = Gesture;
            this.Frame = Frame;
            this.Delta = Delta;
            TimeStamp = DateTime.Now;
        }
    }
}
