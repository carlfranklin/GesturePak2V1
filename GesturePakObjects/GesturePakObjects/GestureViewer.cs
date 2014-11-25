using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.Kinect;
using System.Windows;

namespace GesturePak
{
    
    public class GestureViewer : INotifyPropertyChanged 
    {
        private bool drawing = false;

        public delegate void DrawingFrameEventHandler(object sender, DrawingFrameEventArgs e);
        public event DrawingFrameEventHandler DrawingFrame;
        public event DrawingFrameEventHandler FrameDrawn;
        public event PropertyChangedEventHandler PropertyChanged;
        
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected virtual void OnDrawingFrame(DrawingFrameEventArgs e)
        {
            if (DrawingFrame != null)
                DrawingFrame(this, e);
        }

        protected virtual void OnFrameDrawn(DrawingFrameEventArgs e)
        {
            if (FrameDrawn != null)
                FrameDrawn(this, e);
        }

        public bool DrawHandsEnabled { get; set; }

        private Brush notTrackingJointBrush = new SolidColorBrush(Color.FromArgb(165, 0, 180, 0));
        /// <summary>
        /// Brush used for drawing joints that are currently tracked
        /// </summary>
        public Brush NotTrackingJointBrush
        {
            get { return notTrackingJointBrush; }
            set
            {
                notTrackingJointBrush = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Brush used for drawing joints that are being tracked
        /// </summary>
        private Brush trackingJointBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
        public Brush TrackingJointBrush
        {
            get { return trackingJointBrush; }
            set
            {
                trackingJointBrush = value;
                NotifyPropertyChanged();
            }
        }

        string mouseOverJointName = "";
        /// <summary>
        /// The name of the joint that the mouse is over
        /// </summary>
        public String MouseOverJointName
        {
            get { return mouseOverJointName; }
            set
            {
                mouseOverJointName = value;
                NotifyPropertyChanged();
            }
        }


        private Brush handClosedBrush = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0));
        /// <summary>
        /// Brush used for drawing hands that are currently tracked as closed
        /// </summary>
        public Brush HandClosedBrush
        {
            get { return handClosedBrush; }
            set
            {
                handClosedBrush = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Draw the body from the current frame
        /// </summary>
        public void Refresh()
        {
            if (animationIndex < 0) return;
            var frame = loadedGesture.Frames[animationIndex];
            if (frame == null) return;
            drawBodyFromFrame(frame);
        }

        /// <summary>
        /// Get the Joint Index based on where the mouse cursor is
        /// </summary>
        /// <returns></returns>
        public int JointNumberFromCursor()
        {
            if (CurrentJointPoints == null) return -1;

            Point mp = MouseUtilities.CorrectGetPosition(ImageControl);
            int w = (int)JointThickness * 2;
            mp.X -= JointThickness;
            mp.Y -= JointThickness;

            Rect r = new Rect(mp, new Size(w, w));

            foreach (var jp in CurrentJointPoints)
            {
                Point pt = jp.Value;
                if (r.Contains(pt))
                {
                    return (int)jp.Key;
                }
            }

            return -1;
        }

        private Brush handOpenBrush = new SolidColorBrush(Color.FromArgb(128, 0, 255, 0));
        /// <summary>
        /// Brush used for drawing hands that are currently tracked as opened
        /// </summary>
        public Brush HandOpenBrush
        {
            get { return handOpenBrush; }
            set
            {
                handOpenBrush = value;
                NotifyPropertyChanged();
            }
        }

        private Brush handLassoBrush = new SolidColorBrush(Color.FromArgb(128, 0, 0, 255));
        /// <summary>
        /// Brush used for drawing hands that are currently tracked as in lasso (pointer) position
        /// </summary>
        public Brush HandLassoBrush
        {
            get { return handLassoBrush; }
            set
            {
                handLassoBrush = value;
                NotifyPropertyChanged();
            }
        }

        private Pen bonePen = new Pen(Brushes.Green, 6);
        /// <summary>
        /// Pen used for drawing bones that are currently tracked
        /// </summary>
        public Pen BonePen
        {
            get { return bonePen; }
            set
            {
                bonePen = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Drawing group for body rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Width of display (depth space)
        /// </summary>
        private int displayWidth;

        /// <summary>
        /// Height of display (depth space)
        /// </summary>
        private int displayHeight;

        private DrawingImage imageSource = null;
        ///// <summary>
        ///// Gets the bitmap to display
        ///// </summary>
        public ImageSource ImageSource
        {
            get
            {
                return this.imageSource;
            }
        }

        private Dictionary<JointType, Point> CurrentJointPoints;

        // Reference to the loaded gesture
        private Gesture loadedGesture = null;
        /// <summary>
        /// Get the currently loaded gesture
        /// </summary>
        public Gesture LoadedGesture
        {
            get { return loadedGesture; }
            set
            {
                loadedGesture = value;
                
                loadedGesture.ImageSource = imageSource;
                if (AnimateAllFrames)
                    animationFrames = loadedGesture.StaticFrames();
                else
                    animationFrames = loadedGesture.FramesToMatch();

                SetAnimationIndex(0);

                NotifyPropertyChanged();
            }
        }

        public List<GesturePak.Frame> animationFrames;

        private double handSize = 30;
        /// <summary>
        /// Radius of drawn hand circles
        /// </summary>
        public double HandSize
        {
            get { return handSize; }
            set
            {
                handSize = value;
                NotifyPropertyChanged();
            }
        }

        private bool allowMouseWheelScrub = true;
        /// <summary>
        /// Gets and sets whether mouse wheel scrubbing is enabled
        /// </summary>
        public bool AllowMouseWheelScrub
        {
            get { return allowMouseWheelScrub; }
            set
            {
                allowMouseWheelScrub = value;
                NotifyPropertyChanged();
            }
        }

        private bool animateAllFrames = true;
        /// <summary>
        /// Gets and sets whether to animate all frames or just the matched frames
        /// </summary>
        public bool AnimateAllFrames
        {
            get { return animateAllFrames; }
            set
            {
                animateAllFrames = value;
                if (loadedGesture != null)
                {
                    SetAnimationIndex(0);
                    if (animateAllFrames)
                        animationFrames = loadedGesture.StaticFrames();
                    else
                        animationFrames = loadedGesture.FramesToMatch();
                    NotifyPropertyChanged("TotalFrames");
                    Refresh();
                }
                NotifyPropertyChanged();
            }
        }

        private bool allowJointSelection = true;
        /// <summary>
        /// Gets and sets whether to allow the user to select a joint by clicking on it
        /// </summary>
        public bool AllowJointSelection
        {
            get { return allowJointSelection; }
            set
            {
                allowJointSelection = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Returns the current frame number
        /// </summary>
        public int CurrentFrameNumber
        {
            get { return animationIndex + 1; }
        }

        /// <summary>
        /// Sets the current frame number
        /// </summary>
        /// <param name="value"></param>
        void SetAnimationIndex(int value)
        {
            animationIndex = value;
            NotifyPropertyChanged("CurrentFrameNumber");
        }

        /// <summary>
        /// Returns the total number of frames
        /// </summary>
        public int TotalFrames
        {
            get { return animationFrames.Count; }
        }

        private double jointThickness = 3;
        /// <summary>
        /// Thickness of drawn joint lines
        /// </summary>
        public double JointThickness
        {
            get { return jointThickness; }
            set
            {
                jointThickness = value;
                NotifyPropertyChanged();
            }
        }

        // Used to display recorded gesture
        private System.Windows.Threading.DispatcherTimer animationTimer;

        // The index of the body frame being shown
        private int animationIndex = 0;


        System.Windows.Controls.Image ImageControl;

        public GestureViewer(System.Windows.Controls.Image ImageControl)
        {
            this.DrawHandsEnabled = true;

            this.ImageControl = ImageControl;
            var kinectSensor = KinectSensor.GetDefault();

            // get the depth (display) extents
            FrameDescription frameDescription = kinectSensor.DepthFrameSource.FrameDescription;
            this.displayWidth = frameDescription.Width;
            this.displayHeight =  frameDescription.Height;
            
            ImageControl.MouseWheel += ImageControl_MouseWheel;
            ImageControl.MouseMove += ImageControl_MouseMove;
            ImageControl.MouseUp += ImageControl_MouseUp;

            animationTimer = new System.Windows.Threading.DispatcherTimer();
            animationTimer.Interval = TimeSpan.FromMilliseconds(30);
            animationTimer.Tick += animationTimer_Tick;

            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            imageSource = new DrawingImage(this.drawingGroup);
            ImageControl.Source = imageSource;
        }

        void ImageControl_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!allowJointSelection) return;
            if (LoadedGesture == null) return;

            int index = JointNumberFromCursor();
            if (index != -1)
            {
                LoadedGesture.JointTrackOptions[index] = !LoadedGesture.JointTrackOptions[index];
                Refresh();
            } 
        }

        void ImageControl_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!allowJointSelection) return;
            if (LoadedGesture == null) return;

            int index = JointNumberFromCursor();
            if (index != -1)
                MouseOverJointName = Gesture.GetJointName(index).ToFriendlyCase();
            else
                MouseOverJointName = "";
        }

        void ImageControl_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (!allowMouseWheelScrub) return;

            if (LoadedGesture == null) return;

            if (e.Delta < 0)
            {
                ShowPreviousFrame();
            }
            else
            {
                ShowNextFrame();
            }
        }

        /// <summary>
        /// Start playing the animation
        /// </summary>
        public void StartAnimation()
        {
            if (loadedGesture == null) return;
            if (animateAllFrames)
                animationFrames = loadedGesture.StaticFrames();
            else
                animationFrames = loadedGesture.FramesToMatch();

            SetAnimationIndex(0);
            animationTimer.Start();
        }

        /// <summary>
        /// Stop playing the animation
        /// </summary>
        public void StopAnimation()
        {
            animationTimer.Stop();
        }

        void animationTimer_Tick(object sender, EventArgs e)
        {
            ShowNextFrame();
        }

        /// <summary>
        /// Get the current frame
        /// </summary>
        /// <returns></returns>
        public GesturePak.Frame CurrentFrame()
        {
            if (loadedGesture == null) return null;
            if (animationFrames == null) return null;

            int index = loadedGesture.Frames.IndexOf(animationFrames[animationIndex]);
            int last = loadedGesture.Frames.Count - 1;
            if (index > 0 && index <= last)
            {
                return loadedGesture.Frames[index];
            }
            else return null;
        }

        /// <summary>
        /// Moves to the next frame
        /// </summary>
        public void ShowNextFrame()
        {
            if (animationFrames == null) return;
            if (!drawing)
            {
                drawing = true;
                
                if (animationIndex == animationFrames.Count - 1)
                    SetAnimationIndex(0);
                else
                    SetAnimationIndex(animationIndex + 1);
                GesturePak.Frame thisFrame = animationFrames[animationIndex];
                drawBodyFromFrame(thisFrame);
                drawing = false;
            }
        }

        /// <summary>
        /// Moves to the previous frame
        /// </summary>
        public void ShowPreviousFrame()
        {
            if (animationFrames == null) return;
            if (!drawing)
            {
                drawing = true;
                if (animationIndex == 0)
                    SetAnimationIndex(animationFrames.Count - 1);
                else
                    SetAnimationIndex(animationIndex - 1);
                GesturePak.Frame thisFrame = animationFrames[animationIndex];
                drawBodyFromFrame(thisFrame);
                drawing = false;
            }
        }

        /// <summary>
        /// Move to a particular frame
        /// </summary>
        /// <param name="FrameNumber"></param>
        public void SetFrameNumber(int FrameNumber)
        {
            if (animationFrames == null) return;
            if (FrameNumber >= animationFrames.Count) return;
            if (FrameNumber <= 0) return;
            if (!drawing)
            {
                drawing = true;
                SetAnimationIndex(FrameNumber - 1);
                GesturePak.Frame thisFrame = animationFrames[animationIndex];
                drawBodyFromFrame(thisFrame);
                drawing = false;
            }
        }

        /// <summary>
        /// Clear the body image being displayed
        /// </summary>
        void clearBodyImage()
        {
            using (DrawingContext dc = this.drawingGroup.Open())
            {
                dc.DrawRectangle(Brushes.Transparent, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
            }
        }

        /// <summary>
        /// High-level code to call when you really only want to draw one body frame at a time.
        /// </summary>
        /// <param name="Frame"></param>
        void drawBodyFromFrame(GesturePak.Frame Frame)
        {
            try
            {
                var mapper = KinectSensor.GetDefault().CoordinateMapper;

                using (DrawingContext dc = this.drawingGroup.Open())
                {

                    // Draw a transparent background to set the render size
                    dc.DrawRectangle(Brushes.Transparent, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));

                    // draw the body
                    var joints = (Dictionary<JointType, Joint>)Frame.Joints;


                    Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();
                    foreach (JointType jointType in joints.Keys)
                    {
                        DepthSpacePoint depthSpacePoint = mapper.MapCameraPointToDepthSpace(joints[jointType].Position);
                        jointPoints[jointType] = new Point(depthSpacePoint.X, depthSpacePoint.Y);
                    }

                    var args = new DrawingFrameEventArgs(Frame, jointPoints, dc);

                    OnDrawingFrame(args);

                    this.DrawBodyGP(Frame, jointPoints, dc);

                    // just draw the hands
                    if (DrawHandsEnabled == true)
                    {
                        this.DrawHand(Frame.LeftHandState, jointPoints[JointType.HandLeft], dc);
                        this.DrawHand(Frame.RightHandState, jointPoints[JointType.HandRight], dc);
                    }

                    OnFrameDrawn(args);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Draws a hand symbol if the hand is tracked: red circle = closed, green circle = opened; blue circle = lasso
        /// </summary>
        /// <param name="handState">state of the hand</param>
        /// <param name="handPosition">position of the hand</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawHand(HandState handState, Point handPosition, DrawingContext drawingContext)
        {
            switch (handState)
            {
                case HandState.Closed:
                    if (this.handClosedBrush != null)
                        drawingContext.DrawEllipse(this.handClosedBrush, null, handPosition, HandSize, HandSize);
                    break;

                case HandState.Open:
                    if (this.handOpenBrush != null)
                     drawingContext.DrawEllipse(this.handOpenBrush, null, handPosition, HandSize, HandSize);
                    break;

                case HandState.Lasso:
                    if (this.handLassoBrush != null)
                        drawingContext.DrawEllipse(this.handLassoBrush, null, handPosition, HandSize, HandSize);
                    break;
            }
        }

        private void DrawBoneGP(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, JointType jointType0, JointType jointType1, DrawingContext drawingContext)
        {
            // We assume all drawn bones are inferred unless BOTH joints are tracked
            try
            {
                Pen drawPen = this.bonePen;
                drawingContext.DrawLine(drawPen, jointPoints[jointType0], jointPoints[jointType1]);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void DrawBodyGP(GesturePak.Frame Frame, IDictionary<JointType, Point> jointPoints, DrawingContext drawingContext)
        {
            try
            {
                var joints = (IReadOnlyDictionary<JointType, Joint>)Frame.Joints;

                CurrentJointPoints = (Dictionary<JointType, Point>)jointPoints;

                // Draw the bones

                //// Torso
                DrawBoneGP(joints, jointPoints, JointType.Head, JointType.Neck, drawingContext);
                DrawBoneGP(joints, jointPoints, JointType.Neck, JointType.SpineShoulder, drawingContext);
                DrawBoneGP(joints, jointPoints, JointType.SpineShoulder, JointType.SpineMid, drawingContext);
                DrawBoneGP(joints, jointPoints, JointType.SpineMid, JointType.SpineBase, drawingContext);
                DrawBoneGP(joints, jointPoints, JointType.SpineShoulder, JointType.ShoulderRight, drawingContext);
                DrawBoneGP(joints, jointPoints, JointType.SpineShoulder, JointType.ShoulderLeft, drawingContext);
                DrawBoneGP(joints, jointPoints, JointType.SpineBase, JointType.HipRight, drawingContext);
                DrawBoneGP(joints, jointPoints, JointType.SpineBase, JointType.HipLeft, drawingContext);

                //// Right Arm    
                DrawBoneGP(joints, jointPoints, JointType.ShoulderRight, JointType.ElbowRight, drawingContext);
                DrawBoneGP(joints, jointPoints, JointType.ElbowRight, JointType.WristRight, drawingContext);
                DrawBoneGP(joints, jointPoints, JointType.WristRight, JointType.HandRight, drawingContext);
                DrawBoneGP(joints, jointPoints, JointType.HandRight, JointType.HandTipRight, drawingContext);
                DrawBoneGP(joints, jointPoints, JointType.WristRight, JointType.ThumbRight, drawingContext);

                //// Left Arm
                DrawBoneGP(joints, jointPoints, JointType.ShoulderLeft, JointType.ElbowLeft, drawingContext);
                DrawBoneGP(joints, jointPoints, JointType.ElbowLeft, JointType.WristLeft, drawingContext);
                DrawBoneGP(joints, jointPoints, JointType.WristLeft, JointType.HandLeft, drawingContext);
                DrawBoneGP(joints, jointPoints, JointType.HandLeft, JointType.HandTipLeft, drawingContext);
                DrawBoneGP(joints, jointPoints, JointType.WristLeft, JointType.ThumbLeft, drawingContext);

                //// Right Leg
                DrawBoneGP(joints, jointPoints, JointType.HipRight, JointType.KneeRight, drawingContext);
                DrawBoneGP(joints, jointPoints, JointType.KneeRight, JointType.AnkleRight, drawingContext);
                DrawBoneGP(joints, jointPoints, JointType.AnkleRight, JointType.FootRight, drawingContext);

                //// Left Leg
                DrawBoneGP(joints, jointPoints, JointType.HipLeft, JointType.KneeLeft, drawingContext);
                DrawBoneGP(joints, jointPoints, JointType.KneeLeft, JointType.AnkleLeft, drawingContext);
                DrawBoneGP(joints, jointPoints, JointType.AnkleLeft, JointType.FootLeft, drawingContext);

                // Draw the joints
                var Gesture = Frame.Parent;
                foreach (JointType jointType in joints.Keys)
                {
                    drawJoint(Gesture.JointTrackOptions[(int)jointType], jointPoints[jointType], drawingContext);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        void drawJoint(bool tracking, Point pt, DrawingContext dc)
        {
            Brush drawBrush = this.notTrackingJointBrush;

            if (tracking)
                drawBrush = this.trackingJointBrush;

            dc.DrawEllipse(drawBrush, null, pt, this.jointThickness, this.jointThickness);
        }

        /// <summary>
        /// Load a gesture asyncronously
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public async Task LoadGestureAsync(string FileName)
        {
            try
            {
                loadedGesture = new Gesture();
                
                await loadedGesture.LoadFileAsync(FileName);

                loadedGesture.ImageSource = imageSource;
                if (AnimateAllFrames)
                    animationFrames = loadedGesture.StaticFrames();
                else
                    animationFrames = loadedGesture.FramesToMatch();

                SetAnimationIndex(0);

            }
            catch (Exception)
            {
                CloseGesture();
                throw;
            }
        }

        /// <summary>
        /// Load a gesture into the viewer
        /// </summary>
        /// <param name="FileName"></param>
        public void LoadGesture(string FileName)
        {
            try
            {
                var gestures = new List<Gesture>();
                loadedGesture = new Gesture(FileName);
                loadedGesture.ImageSource = imageSource;
                if (AnimateAllFrames)
                    animationFrames = loadedGesture.StaticFrames();
                else
                    animationFrames = loadedGesture.FramesToMatch();

                SetAnimationIndex(0);

                gestures.Add(loadedGesture);

                drawBodyFromFrame(loadedGesture.Frames[0]);

            }
            catch (Exception ex)
            {
                CloseGesture();
                throw (ex);
            }
        }

        /// <summary>
        /// Close the gesture
        /// </summary>
        public void CloseGesture()
        {
            if (animationTimer != null)
                animationTimer.Stop();
            loadedGesture.SaveFile();
            loadedGesture = null;
            CurrentJointPoints = null;
            SetAnimationIndex(0);
        }

    }
}
