using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Microsoft.Kinect;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GesturePak
{
    
    public class Frame : ICloneable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The name of the Frame
        /// </summary>
        public string Name { get; set; }

        public Gesture Parent { get; set; }

        public String Tag { get; set; }

        /// <summary>
        /// Returns true if the Frame has been matched
        /// </summary>
        public bool Matched { get; set; }

        public bool MatchMe { get; set; }

        /// <summary>
        /// Timestamp in ticks of when the Frame was matched
        /// </summary>
        public long MatchedTicks { get; set; }

        /// <summary>
        /// The difference between the actual position of the joint and the position in the Frame
        /// </summary>
        public Single Delta { get; set; }

        /// <summary>
        /// State of the left hand (Open, Closed, etc.)
        /// </summary>
        public HandState LeftHandState { get; set; }

        /// <summary>
        /// State of the right hand (Open, Closed, etc.)
        /// </summary>
        public HandState RightHandState { get; set; }

        private Body _body = null;
        /// <summary>
        /// The body associated with this Frame. It only exists if Frame was created from a Body object. I may remove this.
        /// </summary>
        public Body Body { get { return _body; } }

        public IDictionary<JointType, Joint> Joints
        {
            get 
            {
                if (Body != null)
                    return (IDictionary<JointType, Joint>)Body.Joints;
                else
                {
                    var joints = new Dictionary<JointType, Joint>();
                    int counter = 0;
                    for (int i = 0; i < RawData.Count; i += 3)
                    {
                        var j = new Joint();
                        j.JointType = (JointType)counter;
                        CameraSpacePoint p = j.Position;
                        p.X = RawData[i];
                        p.Y = RawData[i + 1];
                        p.Z = RawData[i + 2];
                        j.Position = p;
                        counter += 1;
                        joints.Add(j.JointType, j);
                    }
                    return joints;
                }
            }

        }

        private TimeSpan _durationMin = TimeSpan.FromMilliseconds(0);
        /// <summary>
        /// Minimum amount of time a joint must remain in a Frame in order for it to be matched
        /// </summary>
        public TimeSpan DurationMin
        {
            get { return _durationMin; }
            set { _durationMin = value; }
        }

        private TimeSpan _durationMax = TimeSpan.FromMilliseconds(500);
        /// <summary>
        /// Maximum amount of time a joint can remain in a Frame in order for it to be matched
        /// </summary>
        public TimeSpan DurationMax
        {
            get { return _durationMax; }
            set { _durationMax = value; }
        }

        private void _Frame()
        {
            // called from constructors
            //TrackXAxis = true;
            //TrackYAxis = true;
            _data = new List<Single>();
        }

        public Frame()
        {
            _Frame();
        }

        public Frame(Body Body)
        {
            _Frame();
            _body = Body;
            var posData = new List<Single>();
            foreach (Joint j in Body.Joints.Values)
            {
                posData.Add(j.Position.X);
                posData.Add(j.Position.Y);
                posData.Add(j.Position.Z);
            }
            _data = posData;
            LeftHandState = Body.HandLeftState;
            RightHandState = Body.HandRightState;
        }

        public Frame(List<Single> Data)
        {
            _Frame();
            _data = Data;
        }

        public Frame(byte[] Data)
        {
            _Frame();
            _data = new List<Single>();
            for (int i = 0; i < Data.Length; i += 4)
            {
                _data.Add(BitConverter.ToSingle(Data, i));
            }

        }

        private List<Single> _data;
        public List<Single> RawData
        {
            get { return _data; }
        }

        public byte[] GetBytes()
        {
            byte[] buffer = new byte[_data.Count * 4];
            byte[] sngBytes = null;
            for (int i = 0; i <= _data.Count - 1; i++)
            {
                sngBytes = BitConverter.GetBytes(_data[i]);
                Array.Copy(sngBytes, 0, buffer, i * 4, 4);
            }
            return buffer;
        }

        public object Clone()
        {
            Frame p = new Frame(RawData);
            p.Delta = this.Delta;
            p.DurationMax = this.DurationMax;
            p.DurationMin = this.DurationMin;
            p.Matched = this.Matched;
            p.MatchedTicks = this.MatchedTicks;
            p.Name = this.Name;
            return p;
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
