using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using System.Collections.ObjectModel;

namespace GesturePak
{
    public class GestureMatcher
    {
        private List<Gesture> _gestures;
        public List<Gesture> Gestures
        {
            get { return _gestures; }
            set
            {
                _gestures = new List<Gesture>();
                foreach (Gesture g in value)
                {
                    _gestures.Add(g);
                }
            }
        }

        public GestureMatcher(List<Gesture> Gestures, Body Body)
        {
            this.Gestures = Gestures;
            this.Body = Body;
        }

        public GestureMatcher(List<Gesture> Gestures)
        {
            this.Gestures = Gestures;
        }

        public Body Body {get; set;}

        private bool HandStatesMatch(Gesture gesture, Frame frame, Frame realTimeFrame, bool defaultValue)
        {
            if (gesture.TrackLeftHandState)
            {
                if (frame.LeftHandState == realTimeFrame.Body.HandLeftState)
                    return true;
                else
                    return false;
            }
            else
            {
                if (gesture.TrackRightHandState)
                {
                    if (frame.RightHandState == realTimeFrame.Body.HandRightState)
                        return true;
                    else
                        return false;
                }
                else
                    return defaultValue;
            }
        }


        public Gesture GetMatch()
        {
            if (Gestures == null) return null;
            if (Gestures.Count == 0) return null;
            if (Body == null) return null;

            Single RealTimeFrameValue;
            Single thisValue;
            int SpineIndexX;
            Single XOffset;
            Single YOffset;
            Single ZOffset;

            Gestures.ResetGestureMatchFlags();

            Frame RealTimeFrame = new Frame(Body);
            RealTimeFrame.Delta = 0;

            // Match Frames
            foreach (Gesture Gesture in Gestures)
            {
                bool trackingAxes = Gesture.TrackXAxis || Gesture.TrackYAxis || Gesture.TrackZAxis;
                var Frames = Gesture.FramesToMatch();
                foreach (Frame Frame in Frames)
                {
                    //When true means we have tracked either the X, Y, or Z axis for this Frame
                    bool dirtyflag = false;

                    //  This offset moves the X, Y and Z of the current Frame to the
                    //  X, Y and Z of the gesture based on the position of the spine.
                    SpineIndexX = (int)JointType.SpineMid * 3;
                    XOffset = Frame.RawData[SpineIndexX] - RealTimeFrame.RawData[SpineIndexX];
                    YOffset = Frame.RawData[SpineIndexX + 1] - RealTimeFrame.RawData[SpineIndexX + 1];
                    ZOffset = Frame.RawData[SpineIndexX + 2] - RealTimeFrame.RawData[SpineIndexX + 2];
                    Frame.Delta = 0;

                    // counter is necessary to access Frame.JointTrackOptions, of which there are 25.
                    int counter = 0;

                    // tracking axes?
                    if (trackingAxes)
                    { 
                    
                        // Go through each joint in this Frame
                        for (int JointDataIndexX = 0;
                            JointDataIndexX < Frame.RawData.Count - 4;
                            JointDataIndexX += 3)
                        {
                            // Are we tracking this joint?
                            //if (Frame.JointTrackOptions[counter])
                            if (Gesture.JointTrackOptions[counter])
                            {
                                // yes!
                            

                                if (Gesture.TrackXAxis)
                                {
                                    // Record the delta between the X position of the real time Frame
                                    // with that of the Frame we've iterated to
                                    RealTimeFrameValue = RealTimeFrame.RawData[JointDataIndexX] + XOffset;
                                    thisValue = Frame.RawData[JointDataIndexX];
                                    Frame.Delta += System.Math.Abs(thisValue - RealTimeFrameValue);
                                    dirtyflag = true;
                                }

                                if (Gesture.TrackYAxis)
                                {
                                    // Do the same for the Y axis
                                    RealTimeFrameValue = RealTimeFrame.RawData[JointDataIndexX + 1] + YOffset;
                                    thisValue = Frame.RawData[JointDataIndexX + 1];
                                    Frame.Delta += System.Math.Abs(thisValue - RealTimeFrameValue);
                                    dirtyflag = true;
                                }

                                if (Gesture.TrackZAxis)
                                {
                                    // and for Z
                                    RealTimeFrameValue = RealTimeFrame.RawData[JointDataIndexX + 2] + ZOffset;
                                    thisValue = Frame.RawData[JointDataIndexX + 2];
                                    Frame.Delta += System.Math.Abs(thisValue - RealTimeFrameValue);
                                    dirtyflag = true;
                                }
                            }

                            if (dirtyflag)
                            {
                                // We have a delta
                                if (Frame.Delta <= Gesture.FudgeFactor)
                                {
                                    // Do the hand states match?
                                    if (HandStatesMatch(Gesture, Frame, RealTimeFrame, true))
                                    {
                                        if (!Frame.Matched)
                                        {
                                            Frame.Matched = true;
                                            RealTimeFrame.Delta = Frame.Delta;
                                        }
                                    }
                                }
                            }

                            counter++;
                        }
                    }
                    else
                    {
                        // if we are NOT tracking any axes but we ARE tracking hand state, check hand state only
                        if (HandStatesMatch(Gesture, Frame, RealTimeFrame, false))
                        {
                            if (!Frame.Matched)
                            {
                                Frame.Matched = true;
                                RealTimeFrame.Delta = 0;
                            }
                        }
                    }
                }
            }

            // Match Gestures
            foreach (Gesture Gesture in Gestures)
            {
                var Frames = Gesture.FramesToMatch();
                Frame last = Gesture.LastFrame;
                if (last == null)
                { 
                    // no last Frame recorded.

                    // is this the first Frame matched?
                    if (Frames[0].Matched)
                    {
                        // Yes. Save it as the last matched Frame
                        Gesture.LastFrame = Frames[0];

                        // Get a time stamp
                        Gesture.LastFrame.MatchedTicks = DateTime.Now.Ticks;

                        // Is there only one Frame in this gesture?
                        if (Frames.Count == 1)
                        {
                            // That's it. Match the gesture
                            // Clear the last Frame
                            Gesture.LastFrame = null;
                            Gesture.Matched = true;
                            return Gesture;
                        }
                    }
                }
                else
                {
                    // is this the first Frame matched?
                    if (Frames[0].Matched)
                    {
                        // Yes. Save it as the last matched Frame
                        Gesture.LastFrame = Frames[0];

                        // Get a time stamp
                        Gesture.LastFrame.MatchedTicks = DateTime.Now.Ticks;
                    }
                    else
                    {
                        // Is time up for matching the next Frame?
                        long ElapsedTicks = DateTime.Now.Ticks - last.MatchedTicks;
                        if (ElapsedTicks >= last.DurationMax.Ticks)
                        {
                            // Time's up!
                            // Is this Frame still matched?
                            if (last.Matched)
                            {
                                // Yes.
                                // Is it the first Frame?
                                if (last != Frames[0])
                                {
                                    // No. OK to clear it. 
                                    Gesture.LastFrame = null;
                                }
                            }
                        }
                        else
                        {
                            if (ElapsedTicks >= Gesture.LastFrame.DurationMin.Ticks && 
                                ElapsedTicks < Gesture.LastFrame.DurationMax.Ticks)
                            {
                                // We are within the time window
                                int index = Frames.IndexOf(last);
                                
                                // Is there a next Frame?
                                if (index < Frames.Count-1)
                                {
                                    // Yes! Get the next Frame
                                    Frame NextFrame = Frames[index + 1];
                                    
                                    // Did we match the next Frame?
                                    if (NextFrame.Matched)
                                    {
                                        // Yes!!
                                        // Is the NEXT Frame the last Frame we have to match?
                                        if (NextFrame == Frames.Last())
                                        {
                                            // Yes! We have a match.
                                            // Clear the last Frame
                                            Gesture.LastFrame = null;
                                            // raise the roof!
                                            Gesture.Matched = true;
                                            return Gesture;
                                        }
                                        else
                                        {
                                            // No. Set the LastFrame
                                            Gesture.LastFrame = NextFrame;
                                            Gesture.LastFrame.MatchedTicks = DateTime.Now.Ticks;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
