using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GesturePak
{
    class PoseMatchEventArgs : EventArgs
    {
        public MatchingPose Match { get; set; }
        public Pose Pose { get; set; }

        public PoseMatchEventArgs(MatchingPose Match, Pose Pose)
        {
            this.Match = Match;
            this.Pose = Pose;
        }
    }
}
