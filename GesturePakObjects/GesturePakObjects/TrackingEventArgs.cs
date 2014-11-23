using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GesturePak
{
    class TrackingEventArgs : EventArgs
    {
        public Pose Pose { get; set; }
        public Single Delta { get; set; }

        public TrackingEventArgs(Pose Pose, Single Delta)
        {
            this.Pose = Pose;
            this.Delta = Delta;
        }
    }
}
