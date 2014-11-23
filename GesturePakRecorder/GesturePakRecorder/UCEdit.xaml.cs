using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using GesturePak;
using System.Windows.Threading;

namespace GesturePakRecorder
{
    /// <summary>
    /// Interaction logic for UCEdit.xaml
    /// </summary>
    public partial class UCEdit : SmartUserControl
    {
        GestureViewer Viewer = null;
        bool animating = false;
        

        public string GestureFileName { get; set; }

        public UCEdit()
        {
            InitializeComponent();
            Viewer = new GestureViewer(this.BodyImage);
            Viewer.TrackingJointBrush = Brushes.White;
            Viewer.JointThickness = 6;
            Viewer.BonePen = new Pen(Brushes.Green, 8);
            TrackXCheckbox.Checked += TrackXCheckbox_Checked;
            TrackYCheckbox.Checked += TrackYCheckbox_Checked;
            TrackZCheckbox.Checked += TrackZCheckbox_Checked;
            TrackLeftHandStateCheckbox.Checked += TrackLeftHandStateCheckbox_Checked;
            TrackRightHandStateCheckbox.Checked += TrackRightHandStateCheckbox_Checked;
            FudgeFactorTextBox.TextChanged += FudgeFactorTextBox_TextChanged;
            AnimateMatchedFramesOnlyCheckBox.Checked += AnimateMatchedFramesOnlyCheckBox_Checked;
            AnimateMatchedFramesOnlyCheckBox.Unchecked += AnimateMatchedFramesOnlyCheckBox_Unchecked;
        }

        void AnimateMatchedFramesOnlyCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Viewer.AnimateAllFrames = true;
        }

        void AnimateMatchedFramesOnlyCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Viewer.AnimateAllFrames = false;
            
        }

        void FudgeFactorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Viewer.LoadedGesture == null) return;
            if (FudgeFactorTextBox.Text == "") return;
            //if !IsNumeric(FudgeFactorTextBox.Text) return;
            Viewer.LoadedGesture.FudgeFactor = (float)Convert.ToDouble(FudgeFactorTextBox.Text);
            Viewer.LoadedGesture.SaveFile();
        }

        void TrackRightHandStateCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (Viewer.LoadedGesture == null) return;
            Viewer.LoadedGesture.TrackRightHandState = (bool)TrackRightHandStateCheckbox.IsChecked;
            Viewer.LoadedGesture.SaveFile();
        }

        void TrackLeftHandStateCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (Viewer.LoadedGesture == null) return;
            Viewer.LoadedGesture.TrackLeftHandState = (bool)TrackLeftHandStateCheckbox.IsChecked;
            Viewer.LoadedGesture.SaveFile();
        }

        void TrackZCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (Viewer.LoadedGesture == null) return;
            Viewer.LoadedGesture.TrackZAxis = (bool)TrackZCheckbox.IsChecked;
            Viewer.LoadedGesture.SaveFile();
        }

        void TrackYCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (Viewer.LoadedGesture == null) return;
            Viewer.LoadedGesture.TrackYAxis = (bool)TrackYCheckbox.IsChecked;
            Viewer.LoadedGesture.SaveFile();
        }

        void TrackXCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (Viewer.LoadedGesture == null) return;
            Viewer.LoadedGesture.TrackXAxis = (bool)TrackXCheckbox.IsChecked;
            Viewer.LoadedGesture.SaveFile();
        }

        public void LoadGesture(string filename )
        {
            this.GestureFileName = filename;
            Viewer.PropertyChanged += Viewer_PropertyChanged;
            Viewer.LoadGesture(filename);
            var gesture = Viewer.LoadedGesture;
            TrackXCheckbox.IsChecked = gesture.TrackXAxis;
            TrackYCheckbox.IsChecked = gesture.TrackYAxis;
            TrackZCheckbox.IsChecked = gesture.TrackZAxis;
            TrackLeftHandStateCheckbox.IsChecked = gesture.TrackLeftHandState;
            TrackRightHandStateCheckbox.IsChecked = gesture.TrackRightHandState;
            FudgeFactorTextBox.Text = gesture.FudgeFactor.ToString("#.##");
            this.DataContext = Viewer;
        }


        void Viewer_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentFrameNumber")
            {
                this.CurrentFrameTextBlock.Text = "Frame " + Viewer.CurrentFrameNumber.ToString() + " of " + Viewer.TotalFrames.ToString();
                var frame = Viewer.CurrentFrame();
                if (frame != null)
                {
                    if (frame.MatchMe)
                        this.CurrentFrameTextBlock.Foreground = Brushes.Red;
                    else
                        this.CurrentFrameTextBlock.Foreground = Brushes.AntiqueWhite;
                }
            }
            else if (e.PropertyName == "MouseOverJointName")
            {
                CurrentJointTextBlock.Text = Viewer.MouseOverJointName;
            }

        }

        private void AnimateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!animating)
            {
                animating = true;
                Viewer.StartAnimation();
                AnimateButton.Content = " Stop ";
            }
            else
            {
                animating = false;
                Viewer.StopAnimation();
                AnimateButton.Content = " Animate ";
            }
        }

        private void MatchButton_Click(object sender, RoutedEventArgs e)
        {
            var frame = Viewer.CurrentFrame();
            if (frame != null)
            {
                frame.MatchMe = !frame.MatchMe;
                if (frame.MatchMe)
                    this.CurrentFrameTextBlock.Foreground = Brushes.Red;
                else
                    this.CurrentFrameTextBlock.Foreground = Brushes.AntiqueWhite;
            }

            Viewer.LoadedGesture.SaveFile();

        }

        private void TrimStartButton_Click(object sender, RoutedEventArgs e)
        {
            var frame = Viewer.CurrentFrame();
            if (frame != null)
            {
                int index = Viewer.CurrentFrameNumber - 1;
                // remove Frames zero through index
                for (int i = 0; i <= index; i++)
                {
                    Viewer.LoadedGesture.Frames.RemoveAt(0);
                }
                // save it
                Viewer.LoadedGesture.SaveFile();
                // load it
                Viewer.LoadGesture(Viewer.LoadedGesture.FileName);
            }
        }

        private void TrimEndButton_Click(object sender, RoutedEventArgs e)
        {
            var frame = Viewer.CurrentFrame();
            if (frame != null)
            {
                int index = Viewer.CurrentFrameNumber - 1;
                int last = Viewer.LoadedGesture.Frames.Count - 1;

                if (index > 0 && index <= last)
                {
                    // remove Frames index through last
                    for (int i = index; i <= last; i++)
                    {
                        Viewer.LoadedGesture.Frames.RemoveAt(index);
                    }
                    // save it
                    Viewer.LoadedGesture.SaveFile();
                    // load it
                    Viewer.LoadGesture(Viewer.LoadedGesture.FileName);
                }

            }
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            if (Viewer != null && Viewer.LoadedGesture != null)
                Viewer.LoadedGesture.SaveFile();
            OnDataComplete(new DataCompleteEventArgs("TestGesture", GestureFileName));
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            if (Viewer != null && Viewer.LoadedGesture != null)
                Viewer.LoadedGesture.SaveFile();

            OnDataComplete(new DataCompleteEventArgs("Home", null));
        }
    }
}
