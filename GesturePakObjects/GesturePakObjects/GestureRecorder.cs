using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
        
namespace GesturePak
{
    /// <summary>
    /// Provides a nice wrapper around the process of creating a gesture from Frame and/or Body objects.
    /// </summary>
    public class GestureRecorder
    {
        private Gesture Gesture = null;
        public List<Frame> Frames;

        public GestureRecorder()
        {
            Frames = new List<Frame>();
        }

        public Frame RecordFrame(Body Body)
        {
            var Frame = new Frame(Body);
            Frames.Add(Frame);
            return Frame;
        }

        public Frame RecordFrame(Frame Frame)
        {
            Frames.Add(Frame);
            return Frame;
        }

        public Gesture GetRecordedGesture()
        {
            if (Frames.Count == 0)
                throw new Exception("Record some frames first.");

            Gesture = new Gesture();

            foreach (Frame Frame in Frames)
            {
                Gesture.Frames.Add(Frame);    
            }
            Gesture.Renumber();

            return Gesture;
        }
    }
}
