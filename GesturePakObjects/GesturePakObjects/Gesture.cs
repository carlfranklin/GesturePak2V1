using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Media;
using Microsoft.Kinect;
using System.Xml;
using System.Xml.Linq;
using System.Runtime.CompilerServices;

namespace GesturePak
{
    
    public class Gesture : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Frame> Frames {get; set;}
        public bool Matched { get; set; }
        public Frame LastFrame { get; set; }
        public string FileName { get; set; }
        //public string ObjectData { get; set; }

        private bool trackLeftHandState = false;
        public bool TrackLeftHandState { get { return trackLeftHandState; }
            set { trackLeftHandState = value; 
                  NotifyPropertyChanged(); } }

        private bool trackRightHandState = false;
        public bool TrackRightHandState { get { return trackRightHandState; }
            set { trackRightHandState = value; 
                  NotifyPropertyChanged(); } }

        private Single fudgeFactor = 0.2f;
        public Single FudgeFactor {
            get { return fudgeFactor; }
            set { fudgeFactor = value;
                NotifyPropertyChanged(); }
        }

        private DrawingImage imageSource = null;
        public DrawingImage ImageSource {
            get { return imageSource; }
            set { imageSource = value;
                 NotifyPropertyChanged(); }
        }

        private bool trackXAxis = true;
        public bool TrackXAxis
        {
            get { return trackXAxis; }
            set
            {
                trackXAxis = value;
                NotifyPropertyChanged();
            }
        }

        private bool trackYAxis = true;
        public bool TrackYAxis
        {
            get { return trackYAxis; }
            set
            {
                trackYAxis = value;
                NotifyPropertyChanged();
            }
        }

        private bool trackZAxis = false;
        public bool TrackZAxis
        {
            get { return trackZAxis; }
            set
            {
                trackZAxis = value;
                NotifyPropertyChanged();
            }
        }
        
        private bool[] _jointTrackOptions = new bool[25];
        public bool[] JointTrackOptions
        {
            get { return _jointTrackOptions; }
        }

        public Gesture()
        {
            FudgeFactor = 0.2f;
            Frames = new ObservableCollection<Frame>();
            Frames.CollectionChanged += Frames_CollectionChanged;
            TrackXAxis = true;
            TrackYAxis = true;
        }

        public List<Frame> StaticFrames()
        {
            var frames = new List<Frame>();
            foreach (Frame p in this.Frames)
            {
                frames.Add(p);
            }
            return frames;
        }

        void Frames_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (Frame p in e.NewItems)
                {
                    p.Parent = this;
                }
            }
        }

        public Gesture(string FileName) : this()
        {
            //LoadFileNewFormat(FileName);
            LoadFile(FileName);
        }

        public void AddSnapshot(Body body)
        {
            // create a Frame from the body
            var Frame = new Frame(body);
            // add it to the collection
            Frames.Add(Frame);
            // renumber the Frames (names them according to their position)
            this.Renumber();
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Returns the next Frame in sequence 
        /// </summary>
        /// <param name="Frame"></param>
        /// <returns></returns>
        public Frame GetNextFrame(Frame Frame)
        {
            if (Frame == null) return null;
            if (!Frames.Contains(Frame)) return null;

            int i = Frames.IndexOf(Frame);
            int FrameIndex = i + 1;
            if (FrameIndex > Frames.Count - 1) return null;

            return Frames[FrameIndex];
        }

        /// <summary>
        /// Get the Last Frame
        /// </summary>
        /// <returns></returns>
        public Frame GetLastFrame()
        {
            int FrameIndex = GetLastFrameIndex();
            if (FrameIndex > -1)
                return Frames[FrameIndex];
            else
                return null;
        }

        public List<Frame> FramesToMatch()
        {
            var newList = new List<Frame>();

            for (int i = 0; i < Frames.Count; i ++)
            {
                if (Frames[i].MatchMe == true)
                    newList.Add(Frames[i]);
            }
            return newList;
        }

        public int GetLastFrameIndex()
        {
            for (int i = Frames.Count - 1; i >= 0; i -= 1)
            {
                if (Frames[i].MatchMe == true) return i;
            }
            return -1;
        }


        public void LoadV1File(string FileName)
        {
            try
            {
                var ci = new System.Globalization.CultureInfo("en-us");
                var doc = XDocument.Load(FileName);

                // do some VERY basic checking here
                var elG = doc.Element("gesture");
                if (elG == null)
                {
                    throw new Exception("Could not find <gesture> element.");
                }

                var details = from item in doc.Descendants("gesture")
                              select new
                              {
                                  Name = item.Element("Name").Value,
                                  FudgeFactor = item.Element("FudgeFactor").Value
                              };


                foreach (var G in details)
                {
                    this.Name = G.Name;
                    this.FudgeFactor = Single.Parse(G.FudgeFactor, ci);
                    break;
                }

                var Frames = from item in doc.Descendants("pose")
                             select new
                             {
                                 FrameName = item.Attribute("name").Value,
                                 DurationMax = item.Element("DurationMax").Value,
                                 DurationMin = item.Element("DurationMin").Value,
                                 TrackXAxis = item.Element("TrackXAxis").Value,
                                 TrackYAxis = item.Element("TrackYAxis").Value,
                                 TrackZAxis = item.Element("TrackZAxis").Value,
                                 TrackSpineBase = item.Element("TrackHipCenter").Value,
                                 TrackSpineMid = item.Element("TrackSpine").Value,
                                 //TrackNeck = item.Element("TrackShoulderCenter").Value,
                                 TrackHead = item.Element("TrackHead").Value,
                                 TrackLeftShoulder = item.Element("TrackLeftShoulder").Value,
                                 TrackLeftElbow = item.Element("TrackLeftElbow").Value,
                                 TrackLeftWrist = item.Element("TrackLeftWrist").Value,
                                 TrackLeftHand = item.Element("TrackLeftHand").Value,
                                 TrackRightShoulder = item.Element("TrackRightShoulder").Value,
                                 TrackRightElbow = item.Element("TrackRightElbow").Value,
                                 TrackRightWrist = item.Element("TrackRightWrist").Value,
                                 TrackRightHand = item.Element("TrackRightHand").Value,
                                 TrackLeftHip = item.Element("TrackLeftHip").Value,
                                 TrackLeftKnee = item.Element("TrackLeftKnee").Value,
                                 TrackLeftAnkle = item.Element("TrackLeftAnkle").Value,
                                 TrackLeftFoot = item.Element("TrackLeftFoot").Value,
                                 TrackRightHip = item.Element("TrackRightHip").Value,
                                 TrackRightKnee = item.Element("TrackRightKnee").Value,
                                 TrackRightAnkle = item.Element("TrackRightAnkle").Value,
                                 TrackRightFoot = item.Element("TrackRightFoot").Value,
                                 TrackSpineShoulder = item.Element("TrackShoulderCenter").Value,
                                 //TrackLeftHandTip = item.Element("TrackLeftHandTip").Value,
                                 //TrackLeftThumb = item.Element("TrackLeftThumb").Value,
                                 //TrackRightHandTip = item.Element("TrackRightHandTip").Value,
                                 //TrackRightThumb = item.Element("TrackRightThumb").Value

                                 //LeftHandState = item.Element("LeftHandState").Value,
                                 //RightHandState = item.Element("RightHandState").Value
                             };
                foreach (var P in Frames)
                {
                    Frame Frame = new Frame();
                    Frame.MatchMe = true;
                    Frame.Name = P.FrameName;
                    Frame.DurationMax = TimeSpan.FromTicks(Convert.ToInt64(P.DurationMax));
                    Frame.DurationMin = TimeSpan.FromTicks(Convert.ToInt64(P.DurationMin));
                    Frame.LeftHandState = HandState.Open;
                    Frame.RightHandState = HandState.Open;
                    this.TrackXAxis = Convert.ToBoolean(P.TrackXAxis);
                    this.TrackYAxis = Convert.ToBoolean(P.TrackYAxis);
                    this.TrackZAxis = Convert.ToBoolean(P.TrackZAxis);
                    this.JointTrackOptions[(int)JointType.SpineBase] = Convert.ToBoolean(P.TrackSpineBase);
                    this.JointTrackOptions[(int)JointType.SpineMid] = Convert.ToBoolean(P.TrackSpineMid);
                    this.JointTrackOptions[(int)JointType.Neck] = false; //Convert.ToBoolean(P.TrackNeck);
                    this.JointTrackOptions[(int)JointType.Head] = Convert.ToBoolean(P.TrackHead);
                    this.JointTrackOptions[(int)JointType.ShoulderLeft] = Convert.ToBoolean(P.TrackLeftShoulder);
                    this.JointTrackOptions[(int)JointType.ElbowLeft] = Convert.ToBoolean(P.TrackLeftElbow);
                    this.JointTrackOptions[(int)JointType.WristLeft] = Convert.ToBoolean(P.TrackLeftWrist);
                    this.JointTrackOptions[(int)JointType.HandLeft] = Convert.ToBoolean(P.TrackLeftHand);
                    this.JointTrackOptions[(int)JointType.ShoulderRight] = Convert.ToBoolean(P.TrackRightShoulder);
                    this.JointTrackOptions[(int)JointType.ElbowRight] = Convert.ToBoolean(P.TrackRightElbow);
                    this.JointTrackOptions[(int)JointType.WristRight] = Convert.ToBoolean(P.TrackRightWrist);
                    this.JointTrackOptions[(int)JointType.HandRight] = Convert.ToBoolean(P.TrackRightHand);
                    this.JointTrackOptions[(int)JointType.HipLeft] = Convert.ToBoolean(P.TrackLeftHip);
                    this.JointTrackOptions[(int)JointType.KneeLeft] = Convert.ToBoolean(P.TrackLeftKnee);
                    this.JointTrackOptions[(int)JointType.AnkleLeft] = Convert.ToBoolean(P.TrackLeftAnkle);
                    this.JointTrackOptions[(int)JointType.FootLeft] = Convert.ToBoolean(P.TrackLeftFoot);
                    this.JointTrackOptions[(int)JointType.HipRight] = Convert.ToBoolean(P.TrackRightHip);
                    this.JointTrackOptions[(int)JointType.KneeRight] = Convert.ToBoolean(P.TrackRightKnee);
                    this.JointTrackOptions[(int)JointType.AnkleRight] = Convert.ToBoolean(P.TrackRightAnkle);
                    this.JointTrackOptions[(int)JointType.FootRight] = Convert.ToBoolean(P.TrackRightFoot);
                    this.JointTrackOptions[(int)JointType.SpineShoulder] = Convert.ToBoolean(P.TrackSpineShoulder);
                    this.JointTrackOptions[(int)JointType.HandTipLeft] = false; // Convert.ToBoolean(P.TrackLeftHandTip);
                    this.JointTrackOptions[(int)JointType.ThumbLeft] = false; // Convert.ToBoolean(P.TrackLeftThumb);
                    this.JointTrackOptions[(int)JointType.HandTipRight] = false; // Convert.ToBoolean(P.TrackRightHandTip);
                    this.JointTrackOptions[(int)JointType.ThumbRight] = false; // Convert.ToBoolean(P.TrackRightThumb);

                    this.Frames.Add(Frame);
                }
                //this.Renumber();
                this.FileName = FileName;

                // get the joint data
                foreach (Frame p in this.Frames)
                {
                    var thisFrame = from item in doc.Descendants("gesture").Elements("pose")
                                    where (string)item.Attribute("name") == p.Name
                                    select item;

                    foreach (var tp in thisFrame)
                    {
                        p.RawData.Clear();
                        var tempdata = new List<Single>();
                        foreach (XElement el in tp.Elements())
                        {
                            if (el.Name == "data")
                            {
                                var pt = el.Attribute("point").Value;
                                var sng = Convert.ToSingle(pt);
                                tempdata.Add(sng);
                            }
                        }
                        // re-order the data
                        copyTempData(tempdata, p.RawData, 0); // Hip Center, now Spine Base
                        copyTempData(tempdata, p.RawData, 1); // Spine, now Spine Mid
                        copyTempData(tempdata, p.RawData, 2); // Shoulder Center, now Neck
                        copyTempData(tempdata, p.RawData, 3); // Head
                        copyTempData(tempdata, p.RawData, 4); // Left Shoulder
                        copyTempData(tempdata, p.RawData, 5); // Left Elbow
                        copyTempData(tempdata, p.RawData, 6); // Left Wrist
                        copyTempData(tempdata, p.RawData, 7); // Left Hand
                        copyTempData(tempdata, p.RawData, 8); // Right Shoulder
                        copyTempData(tempdata, p.RawData, 9); // Right Elbow
                        copyTempData(tempdata, p.RawData, 10); // Right Wrist
                        copyTempData(tempdata, p.RawData, 11); // Right Hand
                        copyTempData(tempdata, p.RawData, 12); // Left Hip
                        copyTempData(tempdata, p.RawData, 13); // Left Knee
                        copyTempData(tempdata, p.RawData, 14); // Left Ankle
                        copyTempData(tempdata, p.RawData, 15); // Left Foot
                        copyTempData(tempdata, p.RawData, 16); // Right Hip
                        copyTempData(tempdata, p.RawData, 17); // Right Knee
                        copyTempData(tempdata, p.RawData, 18); // Right Ankle
                        copyTempData(tempdata, p.RawData, 19); // Right Foot
                        copyTempData(tempdata, p.RawData, 2); // Shoulder Center should be same X as spine Same Y as shoulders
                        // Copy HandTipLeft and ThumbLeft from the Left Hand
                        copyTempData(tempdata, p.RawData, 7);
                        copyTempData(tempdata, p.RawData, 7);
                        /// Copy HandTipRight and ThumbRight from the Right Hand
                        copyTempData(tempdata, p.RawData, 11);
                        copyTempData(tempdata, p.RawData, 11);

                        // Alter Shoulder Center
                        int leftshoulder = 4 * 3;
                        int rightshoulder = 8 * 3;
                        int shouldercenter = 20 * 3;

                        // shoulder center X should be halfway between shoulder left and shoulder right X
                        Single leftx = p.RawData[leftshoulder] + 1000; // add 1000 to avoid errors with neg numbers.
                        Single rightx = p.RawData[rightshoulder] + 1000;
                        Single shoulderXDiff = Math.Abs(rightx - leftx);
                        p.RawData[shouldercenter] = p.RawData[leftshoulder] + (shoulderXDiff / 2);

                        // shoulder center Y should be halfway between shoulder left and shoulder right Y
                        Single lefty = p.RawData[leftshoulder + 1] + 1000;
                        Single righty = p.RawData[rightshoulder + 1] + 1000;
                        Single shoulderYDiff = Math.Abs(righty - lefty);
                        if (righty > lefty)
                            p.RawData[shouldercenter + 1] = p.RawData[leftshoulder + 1] + (shoulderYDiff / 2);
                        else
                            p.RawData[shouldercenter + 1] = p.RawData[rightshoulder + 1] + (shoulderYDiff / 2);

                        //p.RawData[shouldercenter + 1] = p.RawData[leftshoulder + 1];

                        // Bring the spine mid up halfway between the spine base and shouldercenter
                        //p.RawData[spine + 1] += .1f;


                        break;
                    }
                }
                this.Renumber();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void copyTempData(List<Single> source, List<Single> dest, int JointIndex)
        {
            int index = JointIndex * 3;
            dest.Add(source[index]);
            dest.Add(source[index + 1]);
            dest.Add(source[index + 2]);
        }

        public async Task LoadFileAsync(string FileName)
        {

            Task t = Task.Run(() =>
            {
                LoadFile(FileName);
            });
            await t;
        }

        public void LoadFile(string FileName)
        {
            try
            {
                var ci = new System.Globalization.CultureInfo("en-us");
                var doc = XDocument.Load(FileName);

                // do some VERY basic checking here
                var elOldG = doc.Element("gesture");
                if (elOldG != null)
                {
                    //throw new Exception("This gesture looks like it was created with GesturePak 1.0, and is therefore incompatible. Try making a new gesture.");
                    LoadV1File(FileName);
                    return;
                }

                string version = "";

                var elG = doc.Element("Gesture");
                if (elG != null)
                {
                    var ver = elG.Element("Version");
                    if (ver == null)
                    {
                        throw new Exception("Incompatible version.");
                    }
                    else
                        version = ver.Value.ToString();
                }
                else
                {
                    throw new Exception("Could not find <Gesture> element.");
                }

                if (version == "2.0")
                {
                    var details = from item in doc.Descendants("Gesture")
                                  select new
                                  {
                                      Name = item.Element("Name").Value,
                                      ver = item.Element("Version"),
                                      FudgeFactor = item.Element("FudgeFactor").Value,
                                      TrackXAxis = item.Element("TrackXAxis").Value,
                                      TrackYAxis = item.Element("TrackYAxis").Value,
                                      TrackZAxis = item.Element("TrackZAxis").Value,
                                      TrackLeftHandState = item.Element("TrackLeftHandState").Value,
                                      TrackRightHandState = item.Element("TrackRightHandState").Value,
                                      TrackSpineBase = item.Element("TrackSpineBase").Value,
                                      TrackSpineMid = item.Element("TrackSpineMid").Value,
                                      TrackNeck = item.Element("TrackNeck").Value,
                                      TrackHead = item.Element("TrackHead").Value,
                                      TrackLeftShoulder = item.Element("TrackLeftShoulder").Value,
                                      TrackLeftElbow = item.Element("TrackLeftElbow").Value,
                                      TrackLeftWrist = item.Element("TrackLeftWrist").Value,
                                      TrackLeftHand = item.Element("TrackLeftHand").Value,
                                      TrackRightShoulder = item.Element("TrackRightShoulder").Value,
                                      TrackRightElbow = item.Element("TrackRightElbow").Value,
                                      TrackRightWrist = item.Element("TrackRightWrist").Value,
                                      TrackRightHand = item.Element("TrackRightHand").Value,
                                      TrackLeftHip = item.Element("TrackLeftHip").Value,
                                      TrackLeftKnee = item.Element("TrackLeftKnee").Value,
                                      TrackLeftAnkle = item.Element("TrackLeftAnkle").Value,
                                      TrackLeftFoot = item.Element("TrackLeftFoot").Value,
                                      TrackRightHip = item.Element("TrackRightHip").Value,
                                      TrackRightKnee = item.Element("TrackRightKnee").Value,
                                      TrackRightAnkle = item.Element("TrackRightAnkle").Value,
                                      TrackRightFoot = item.Element("TrackRightFoot").Value,
                                      TrackSpineShoulder = item.Element("TrackSpineShoulder").Value,
                                      TrackLeftHandTip = item.Element("TrackLeftHandTip").Value,
                                      TrackLeftThumb = item.Element("TrackLeftThumb").Value,
                                      TrackRightHandTip = item.Element("TrackRightHandTip").Value,
                                      TrackRightThumb = item.Element("TrackRightThumb").Value
                                  };


                    foreach (var G in details)
                    {
                        this.Name = G.Name;
                        this.FudgeFactor = Single.Parse(G.FudgeFactor, ci);
                        this.TrackXAxis = Convert.ToBoolean(G.TrackXAxis);
                        this.TrackYAxis = Convert.ToBoolean(G.TrackYAxis);
                        this.TrackZAxis = Convert.ToBoolean(G.TrackZAxis);
                        this.TrackLeftHandState = Convert.ToBoolean(G.TrackLeftHandState);
                        this.TrackRightHandState = Convert.ToBoolean(G.TrackRightHandState);
                        this.JointTrackOptions[(int)JointType.SpineBase] = Convert.ToBoolean(G.TrackSpineBase);
                        this.JointTrackOptions[(int)JointType.SpineMid] = Convert.ToBoolean(G.TrackSpineMid);
                        this.JointTrackOptions[(int)JointType.Neck] = Convert.ToBoolean(G.TrackNeck);
                        this.JointTrackOptions[(int)JointType.Head] = Convert.ToBoolean(G.TrackHead);
                        this.JointTrackOptions[(int)JointType.ShoulderLeft] = Convert.ToBoolean(G.TrackLeftShoulder);
                        this.JointTrackOptions[(int)JointType.ElbowLeft] = Convert.ToBoolean(G.TrackLeftElbow);
                        this.JointTrackOptions[(int)JointType.WristLeft] = Convert.ToBoolean(G.TrackLeftWrist);
                        this.JointTrackOptions[(int)JointType.HandLeft] = Convert.ToBoolean(G.TrackLeftHand);
                        this.JointTrackOptions[(int)JointType.ShoulderRight] = Convert.ToBoolean(G.TrackRightShoulder);
                        this.JointTrackOptions[(int)JointType.ElbowRight] = Convert.ToBoolean(G.TrackRightElbow);
                        this.JointTrackOptions[(int)JointType.WristRight] = Convert.ToBoolean(G.TrackRightWrist);
                        this.JointTrackOptions[(int)JointType.HandRight] = Convert.ToBoolean(G.TrackRightHand);
                        this.JointTrackOptions[(int)JointType.HipLeft] = Convert.ToBoolean(G.TrackLeftHip);
                        this.JointTrackOptions[(int)JointType.KneeLeft] = Convert.ToBoolean(G.TrackLeftKnee);
                        this.JointTrackOptions[(int)JointType.AnkleLeft] = Convert.ToBoolean(G.TrackLeftAnkle);
                        this.JointTrackOptions[(int)JointType.FootLeft] = Convert.ToBoolean(G.TrackLeftFoot);
                        this.JointTrackOptions[(int)JointType.HipRight] = Convert.ToBoolean(G.TrackRightHip);
                        this.JointTrackOptions[(int)JointType.KneeRight] = Convert.ToBoolean(G.TrackRightKnee);
                        this.JointTrackOptions[(int)JointType.AnkleRight] = Convert.ToBoolean(G.TrackRightAnkle);
                        this.JointTrackOptions[(int)JointType.FootRight] = Convert.ToBoolean(G.TrackRightFoot);
                        this.JointTrackOptions[(int)JointType.SpineShoulder] = Convert.ToBoolean(G.TrackSpineShoulder);
                        this.JointTrackOptions[(int)JointType.HandTipLeft] = Convert.ToBoolean(G.TrackLeftHandTip);
                        this.JointTrackOptions[(int)JointType.ThumbLeft] = Convert.ToBoolean(G.TrackLeftThumb);
                        this.JointTrackOptions[(int)JointType.HandTipRight] = Convert.ToBoolean(G.TrackRightHandTip);
                        this.JointTrackOptions[(int)JointType.ThumbRight] = Convert.ToBoolean(G.TrackRightThumb);
                        break;
                    }

                    var Frames = from item in doc.Descendants("Frame")
                                 select new
                                 {
                                     FrameName = item.Attribute("Name").Value,
                                     MatchMe = item.Attribute("Match").Value,
                                     DurationMax = item.Element("DurationMax").Value,
                                     DurationMin = item.Element("DurationMin").Value,
                                     LeftHandState = item.Element("LeftHandState").Value,
                                     RightHandState = item.Element("RightHandState").Value
                                 };

                    foreach (var P in Frames)
                    {
                        Frame Frame = new Frame();

                        Frame.Name = P.FrameName;
                        Frame.MatchMe = Convert.ToBoolean(P.MatchMe);
                        Frame.DurationMax = TimeSpan.FromTicks(Convert.ToInt64(P.DurationMax));
                        Frame.DurationMin = TimeSpan.FromTicks(Convert.ToInt64(P.DurationMin));
                        Frame.LeftHandState = (HandState)Convert.ToInt32(P.LeftHandState);
                        Frame.RightHandState = (HandState)Convert.ToInt32(P.RightHandState);

                        this.Frames.Add(Frame);
                    }
                    this.FileName = FileName;

                    // get the joint data
                    foreach (Frame p in this.Frames)
                    {

                        var thisFrame = from item in doc.Descendants("Gesture").Elements("Frame")
                                        where (string)item.Attribute("Name") == p.Name
                                        select item;

                        var names = GetAllJointNames();
                        var joints = (Dictionary<JointType, Joint>)p.Joints;

                        // initialize the RawData array
                        p.RawData.Clear();
                        for (int i = 0; i < 75; i++)
                        {
                            p.RawData.Add(0f);
                        }

                        foreach (var tp in thisFrame)
                        {
                            foreach (XElement el in tp.Elements())
                            {
                                var jointName = el.Name.ToString();
                                if (names.Contains(jointName))
                                {
                                    var jt = GetJointTypeByName(jointName);
                                    int index = (int)jt * 3;
                                    p.RawData[index] = Convert.ToSingle(el.Element("X").Attribute("Value").Value);
                                    p.RawData[index + 1] = Convert.ToSingle(el.Element("Y").Attribute("Value").Value);
                                    p.RawData[index + 2] = Convert.ToSingle(el.Element("Z").Attribute("Value").Value);
                                }
                            }
                        }

                    }
                }
                else if (version == "2.1")
                {
                    var details = from item in doc.Descendants("Gesture")
                                  select new
                                  {
                                      Name = item.Element("Name").Value,
                                      ver = item.Element("Version"),
                                      FudgeFactor = item.Element("FudgeFactor").Value,
                                      //ObjectData = item.Element("ObjectData").Value,
                                      TrackXAxis = item.Element("TrackXAxis").Value,
                                      TrackYAxis = item.Element("TrackYAxis").Value,
                                      TrackZAxis = item.Element("TrackZAxis").Value,
                                      TrackLeftHandState = item.Element("TrackLeftHandState").Value,
                                      TrackRightHandState = item.Element("TrackRightHandState").Value,
                                      TrackSpineBase = item.Element("TrackSpineBase").Value,
                                      TrackSpineMid = item.Element("TrackSpineMid").Value,
                                      TrackNeck = item.Element("TrackNeck").Value,
                                      TrackHead = item.Element("TrackHead").Value,
                                      TrackLeftShoulder = item.Element("TrackLeftShoulder").Value,
                                      TrackLeftElbow = item.Element("TrackLeftElbow").Value,
                                      TrackLeftWrist = item.Element("TrackLeftWrist").Value,
                                      TrackLeftHand = item.Element("TrackLeftHand").Value,
                                      TrackRightShoulder = item.Element("TrackRightShoulder").Value,
                                      TrackRightElbow = item.Element("TrackRightElbow").Value,
                                      TrackRightWrist = item.Element("TrackRightWrist").Value,
                                      TrackRightHand = item.Element("TrackRightHand").Value,
                                      TrackLeftHip = item.Element("TrackLeftHip").Value,
                                      TrackLeftKnee = item.Element("TrackLeftKnee").Value,
                                      TrackLeftAnkle = item.Element("TrackLeftAnkle").Value,
                                      TrackLeftFoot = item.Element("TrackLeftFoot").Value,
                                      TrackRightHip = item.Element("TrackRightHip").Value,
                                      TrackRightKnee = item.Element("TrackRightKnee").Value,
                                      TrackRightAnkle = item.Element("TrackRightAnkle").Value,
                                      TrackRightFoot = item.Element("TrackRightFoot").Value,
                                      TrackSpineShoulder = item.Element("TrackSpineShoulder").Value,
                                      TrackLeftHandTip = item.Element("TrackLeftHandTip").Value,
                                      TrackLeftThumb = item.Element("TrackLeftThumb").Value,
                                      TrackRightHandTip = item.Element("TrackRightHandTip").Value,
                                      TrackRightThumb = item.Element("TrackRightThumb").Value
                                  };


                    foreach (var G in details)
                    {
                        this.Name = G.Name;
                        this.FudgeFactor = Single.Parse(G.FudgeFactor, ci);
                        //this.ObjectData = G.ObjectData;
                        this.TrackXAxis = Convert.ToBoolean(G.TrackXAxis);
                        this.TrackYAxis = Convert.ToBoolean(G.TrackYAxis);
                        this.TrackZAxis = Convert.ToBoolean(G.TrackZAxis);
                        this.TrackLeftHandState = Convert.ToBoolean(G.TrackLeftHandState);
                        this.TrackRightHandState = Convert.ToBoolean(G.TrackRightHandState);
                        this.JointTrackOptions[(int)JointType.SpineBase] = Convert.ToBoolean(G.TrackSpineBase);
                        this.JointTrackOptions[(int)JointType.SpineMid] = Convert.ToBoolean(G.TrackSpineMid);
                        this.JointTrackOptions[(int)JointType.Neck] = Convert.ToBoolean(G.TrackNeck);
                        this.JointTrackOptions[(int)JointType.Head] = Convert.ToBoolean(G.TrackHead);
                        this.JointTrackOptions[(int)JointType.ShoulderLeft] = Convert.ToBoolean(G.TrackLeftShoulder);
                        this.JointTrackOptions[(int)JointType.ElbowLeft] = Convert.ToBoolean(G.TrackLeftElbow);
                        this.JointTrackOptions[(int)JointType.WristLeft] = Convert.ToBoolean(G.TrackLeftWrist);
                        this.JointTrackOptions[(int)JointType.HandLeft] = Convert.ToBoolean(G.TrackLeftHand);
                        this.JointTrackOptions[(int)JointType.ShoulderRight] = Convert.ToBoolean(G.TrackRightShoulder);
                        this.JointTrackOptions[(int)JointType.ElbowRight] = Convert.ToBoolean(G.TrackRightElbow);
                        this.JointTrackOptions[(int)JointType.WristRight] = Convert.ToBoolean(G.TrackRightWrist);
                        this.JointTrackOptions[(int)JointType.HandRight] = Convert.ToBoolean(G.TrackRightHand);
                        this.JointTrackOptions[(int)JointType.HipLeft] = Convert.ToBoolean(G.TrackLeftHip);
                        this.JointTrackOptions[(int)JointType.KneeLeft] = Convert.ToBoolean(G.TrackLeftKnee);
                        this.JointTrackOptions[(int)JointType.AnkleLeft] = Convert.ToBoolean(G.TrackLeftAnkle);
                        this.JointTrackOptions[(int)JointType.FootLeft] = Convert.ToBoolean(G.TrackLeftFoot);
                        this.JointTrackOptions[(int)JointType.HipRight] = Convert.ToBoolean(G.TrackRightHip);
                        this.JointTrackOptions[(int)JointType.KneeRight] = Convert.ToBoolean(G.TrackRightKnee);
                        this.JointTrackOptions[(int)JointType.AnkleRight] = Convert.ToBoolean(G.TrackRightAnkle);
                        this.JointTrackOptions[(int)JointType.FootRight] = Convert.ToBoolean(G.TrackRightFoot);
                        this.JointTrackOptions[(int)JointType.SpineShoulder] = Convert.ToBoolean(G.TrackSpineShoulder);
                        this.JointTrackOptions[(int)JointType.HandTipLeft] = Convert.ToBoolean(G.TrackLeftHandTip);
                        this.JointTrackOptions[(int)JointType.ThumbLeft] = Convert.ToBoolean(G.TrackLeftThumb);
                        this.JointTrackOptions[(int)JointType.HandTipRight] = Convert.ToBoolean(G.TrackRightHandTip);
                        this.JointTrackOptions[(int)JointType.ThumbRight] = Convert.ToBoolean(G.TrackRightThumb);
                        break;
                    }

                    var Frames = from item in doc.Descendants("Frame")
                                 select new
                                 {
                                     FrameName = item.Attribute("Name").Value,
                                     MatchMe = item.Attribute("Match").Value,
                                     DurationMax = item.Element("DurationMax").Value,
                                     DurationMin = item.Element("DurationMin").Value,
                                     LeftHandState = item.Element("LeftHandState").Value,
                                     RightHandState = item.Element("RightHandState").Value,
                                     Tag = item.Element("Tag").Value,

                                 };

                    foreach (var P in Frames)
                    {
                        Frame Frame = new Frame();

                        Frame.Name = P.FrameName;
                        Frame.MatchMe = Convert.ToBoolean(P.MatchMe);
                        Frame.DurationMax = TimeSpan.FromTicks(Convert.ToInt64(P.DurationMax));
                        Frame.DurationMin = TimeSpan.FromTicks(Convert.ToInt64(P.DurationMin));
                        Frame.LeftHandState = (HandState)Convert.ToInt32(P.LeftHandState);
                        Frame.RightHandState = (HandState)Convert.ToInt32(P.RightHandState);
                        Frame.Tag = P.Tag;
                        this.Frames.Add(Frame);
                    }
                    this.FileName = FileName;

                    // get the joint data
                    foreach (Frame p in this.Frames)
                    {

                        var thisFrame = from item in doc.Descendants("Gesture").Elements("Frame")
                                        where (string)item.Attribute("Name") == p.Name
                                        select item;

                        var names = GetAllJointNames();
                        var joints = (Dictionary<JointType, Joint>)p.Joints;

                        // initialize the RawData array
                        p.RawData.Clear();
                        for (int i = 0; i < 75; i++)
                        {
                            p.RawData.Add(0f);
                        }

                        foreach (var tp in thisFrame)
                        {
                            foreach (XElement el in tp.Elements())
                            {
                                var jointName = el.Name.ToString();
                                if (names.Contains(jointName))
                                {
                                    var jt = GetJointTypeByName(jointName);
                                    int index = (int)jt * 3;
                                    p.RawData[index] = Convert.ToSingle(el.Element("X").Attribute("Value").Value);
                                    p.RawData[index + 1] = Convert.ToSingle(el.Element("Y").Attribute("Value").Value);
                                    p.RawData[index + 2] = Convert.ToSingle(el.Element("Z").Attribute("Value").Value);
                                }
                            }
                        }

                    }

                }

            }
            catch (Exception)
            {
                throw;
            }
            this.Renumber();

        }

        private JointType GetJointTypeByName(string name)
        {
            for (int i = 0; i < 25; i++)
            {
                if (name == GetJointName(i))
                    return (JointType)i;
            }
            // default
            return JointType.HandRight;
        }
        
        private List<string> GetAllJointNames()
        {
            var names = new List<string>();
            for (int i = 0; i < 25; i++)
                names.Add(GetJointName(i));
            return names;
        }

        public void SaveFile()
        {
            if (FileName != "")
                SaveFile(FileName);
            else
                if (FileName == "") throw new Exception("No File Specified");
        }

        public void SaveFile(string FileName)
        {
            if (FileName == "") throw new Exception("No File Specified");

            var ci = new System.Globalization.CultureInfo("en-us");

            XmlTextWriter xmlwriter = new XmlTextWriter(FileName, Encoding.UTF8);
            xmlwriter.Formatting = Formatting.Indented;
            xmlwriter.WriteStartDocument();

            xmlwriter.WriteStartElement("Gesture");
            xmlwriter.WriteElementString("Name", Name);
            xmlwriter.WriteElementString("Version", "2.1");
            xmlwriter.WriteElementString("FudgeFactor", FudgeFactor.ToString(ci));
            //xmlwriter.WriteElementString("ObjectData", ObjectData);
            xmlwriter.WriteElementString("TrackXAxis", TrackXAxis.ToString());
            xmlwriter.WriteElementString("TrackYAxis", TrackYAxis.ToString());
            xmlwriter.WriteElementString("TrackZAxis", TrackZAxis.ToString());
            xmlwriter.WriteElementString("TrackLeftHandState", TrackLeftHandState.ToString());
            xmlwriter.WriteElementString("TrackRightHandState", TrackRightHandState.ToString());
            for (int i = 0; i < 25; i++)
            {
                xmlwriter.WriteElementString("Track" + GetJointName(i), JointTrackOptions[i].ToString());
            }

            foreach (Frame p in Frames)
            {
                xmlwriter.WriteStartElement("Frame");
                xmlwriter.WriteAttributeString("Name", p.Name);

                xmlwriter.WriteAttributeString("Match", p.MatchMe.ToString());

                xmlwriter.WriteAttributeString("xml:lang", "en");

                xmlwriter.WriteElementString("DurationMax", p.DurationMax.Ticks.ToString());
                xmlwriter.WriteElementString("DurationMin", p.DurationMin.Ticks.ToString());

                xmlwriter.WriteElementString("LeftHandState", Convert.ToInt32(p.LeftHandState).ToString());
                xmlwriter.WriteElementString("RightHandState", Convert.ToInt32(p.RightHandState).ToString());

                xmlwriter.WriteElementString("Tag", (p.Tag));

                int jointCount = 0;
                for (int i = 0; i < p.RawData.Count; i += 3)
                {
                    string jointName = GetJointName(jointCount);
                    xmlwriter.WriteStartElement(jointName);

                    xmlwriter.WriteStartElement("X");
                    xmlwriter.WriteAttributeString("Value", p.RawData[i].ToString(ci));
                    xmlwriter.WriteEndElement();

                    xmlwriter.WriteStartElement("Y");
                    xmlwriter.WriteAttributeString("Value", p.RawData[i + 1].ToString(ci));
                    xmlwriter.WriteEndElement();

                    xmlwriter.WriteStartElement("Z");
                    xmlwriter.WriteAttributeString("Value", p.RawData[i + 2].ToString(ci));
                    xmlwriter.WriteEndElement();

                    xmlwriter.WriteEndElement();

                    jointCount++;
                }
                xmlwriter.WriteEndElement();
            }

            xmlwriter.WriteEndElement();
            xmlwriter.WriteEndDocument();
            xmlwriter.Flush();
            xmlwriter.Close();
        }

        public static string GetJointName(int i)
        {
            string jn = ((JointType)i).ToString();
            if (jn.EndsWith("Left"))
            {
                jn = "Left" + jn.Substring(0, jn.Length - 4);
            }
            else
            {
                if (jn.EndsWith("Right"))
                {
                    jn = "Right" + jn.Substring(0, jn.Length - 5);
                }
            }
            return jn;
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
