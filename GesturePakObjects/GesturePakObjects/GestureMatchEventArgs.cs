using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GesturePak
{
    class GestureMatchEventArgs : EventArgs
    {
        public Gesture Gesture { get; set; }
        public GestureMatchEventArgs(Gesture Gesture)
        {
            this.Gesture = Gesture;
        }
    }
}
