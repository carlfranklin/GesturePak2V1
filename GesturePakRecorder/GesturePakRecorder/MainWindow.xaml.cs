namespace GesturePakRecorder
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    using System.Windows.Input;
    using KinectTools;
    using System.Windows.Controls;
    using System.Runtime.CompilerServices;
    using System.Collections.ObjectModel;
    using System.Windows.Media.Animation;
    using GesturePak;
    using Microsoft.Win32;
    using System.Speech.Recognition;

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        double OriginalHeight = 0;
        double OriginalWidth = 0;

        string GesturePakFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\GesturePak";

        ColorAndBodyViewer KinectViewer = null;
        SmartUserControl KinectHappyUserControl = null;
        NavPageInfo LastPage = null;
        NavPageInfo CurrentPage = null;

        GestureRecorder Recorder = null;

        Storyboard AnimateLeftToRight = null;
        Storyboard AnimateRightToLeft = null;
        Storyboard FlyOffLeft = null;
        Storyboard FlyOffRight = null;

        int selectAndFlyIn = -1;
        int selectAndFlyOut = -1;

        UCHome HomeControl;
        UCPreRecording PreRecordingControl;
        UCRecorder RecordingControl;
        UCEdit EditControl;
        UCTest TestControl;

        NavPageInfo HomePage = null;
        NavPageInfo PreRecordPage = null;
        NavPageInfo RecordPage = null;
        NavPageInfo EditPage = null;
        NavPageInfo TestPage = null;

        bool Recording = false;
        bool Testing = false;

        string MainWindowPhrases = "Record\r\nLoad\r\nInteract";

        //private UserControl current = null;

        //private SpeechTools.SpeechListener listener = null;

        private ObservableCollection<NavPageInfo> uipages = null;
        public ObservableCollection<NavPageInfo> UIPages
        {
            get
            {
                if (uipages == null)
                    uipages = new ObservableCollection<NavPageInfo>();

                return uipages;
            }
            set
            {
                uipages = value;
                NotifyPropertyChanged();
            }
        }

        public int pageIndex { get; set; }
        public int lastNavIndex { get; set; }



        public MainWindow()
        {
            InitializeComponent();
            this.Style = (Style)Resources["GradientStyle"];
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;

            HomeControl = new UCHome();
            PreRecordingControl = new UCPreRecording();
            RecordingControl = new UCRecorder();
            EditControl = new UCEdit();
            TestControl = new UCTest();

            HomePage = new NavPageInfo(HomeControl, false, "Home");
            PreRecordPage = new NavPageInfo(PreRecordingControl, false, "Before You Record ...");
            RecordPage = new NavPageInfo(RecordingControl, false, "Record Gesture");
            EditPage = new NavPageInfo(EditControl, false, "Edit Gesture");
            TestPage = new NavPageInfo(TestControl, false, "Test Gesture");

            UIPages.Add(HomePage);
            UIPages.Add(PreRecordPage);
            UIPages.Add(RecordPage);
            UIPages.Add(EditPage);
            UIPages.Add(TestPage);

            // name the controls
            for (int i = 0; i < UIPages.Count; i++)
            {
                UIPages[i].UserControl.Name = "UC" + i.ToString();
                UIPages[i].UserControl.PropertyChanged += UserControl_PropertyChanged;
                UIPages[i].UserControl.DataComplete += UserControl_DataComplete;
            }

            AnimateLeftToRight = ((Storyboard)this.Resources["AnimateLeftToRight"]);
            AnimateRightToLeft = ((Storyboard)this.Resources["AnimateRightToLeft"]);

            FlyOffLeft = ((Storyboard)this.Resources["FlyOffLeft"]);
            FlyOffRight = ((Storyboard)this.Resources["FlyOffRight"]);
            FlyOffLeft.Completed += FlyOffLeft_Completed;
            FlyOffRight.Completed += FlyOffRight_Completed;

            pageIndex = 0;
            FlyIn();
        }

        void LoadGesture()
        {
            OpenFileDialog openDialog = new OpenFileDialog()
            {
                InitialDirectory = GesturePakFolder,
                FileName = "*.xml"
            };
            if (openDialog.ShowDialog() == true)
            {
                EditControl.LoadGesture(openDialog.FileName);
                MoveTo(EditPage);
            }
        }

        void UserControl_DataComplete(object sender, DataCompleteEventArgs e)
        {
            Testing = false;
            
            switch (e.Title)
            {
                case "Back" :
                    if (this.LastPage != null)
                        MoveTo(this.LastPage);
                    break;
                case "Home" :
                    MoveTo(HomePage);
                    break;
                case "Edit" :
                    MoveTo(EditPage);
                    break;
                case "Load":
                    LoadGesture();
                    break;
                case "Pre-Record":
                    MoveTo(PreRecordPage);
                    break;
                case "Interact":
                    TestControl.TopLabel = "Play time!";
                    MoveTo(TestPage);
                    Testing = false;
                    break;
                case "RecordPage":
                    RecordingControl.GestureSeconds = Convert.ToInt32(PreRecordingControl.GestureSecondsTextBox.Text);
                    MoveTo(RecordPage);
                    break;
                case "CancelRecord":
                    MoveTo(HomePage);
                    break;
                case "StartRecording":
                    GestureName = PreRecordingControl.GestureNameTextBox.Text;
                    Recording = true;
                    Recorder.StartRecording();
                    //KinectViewer.WriteJpgFiles = true;
                    break;
                case "StopRecording":
                    //KinectViewer.WriteJpgFiles = false;
                    Recording = false;
                    Recorder.StopRecording();
                    var gesture = Recorder.GetRecordedGesture();
                    gesture.Name = GestureName;
                    
                    var filename = GesturePakFolder + "\\" + gesture.Name + ".xml";

                    gesture.TrackLeftHandState = (bool)PreRecordingControl.TrackLeftHandStateCheckbox.IsChecked;
                    gesture.TrackRightHandState = (bool)PreRecordingControl.TrackRightHandStateCheckbox.IsChecked;
                    gesture.TrackXAxis = (bool)PreRecordingControl.TrackXCheckbox.IsChecked;
                    gesture.TrackYAxis = (bool)PreRecordingControl.TrackYCheckbox.IsChecked;
                    gesture.TrackZAxis = (bool)PreRecordingControl.TrackZCheckbox.IsChecked;
                    gesture.SaveFile(filename);
                    RecordingControl.GestureFileName = filename;
                    EditControl.LoadGesture(filename);
                    MoveTo(EditPage);

                    //SaveFileDialog dialog = new SaveFileDialog()
                    //{
                    //    InitialDirectory = GesturePakFolder,
                    //    FileName = filename,
                    //    Filter = "Gesture Files(*.xml)|*.xml|All(*.*)|*"
                    //};

                    //if (dialog.ShowDialog() == true)
                    //{
                    //    gesture.TrackLeftHandState = (bool)PreRecordingControl.TrackLeftHandStateCheckbox.IsChecked;
                    //    gesture.TrackRightHandState = (bool)PreRecordingControl.TrackRightHandStateCheckbox.IsChecked;
                    //    gesture.TrackXAxis = (bool)PreRecordingControl.TrackXCheckbox.IsChecked;
                    //    gesture.TrackYAxis = (bool)PreRecordingControl.TrackYCheckbox.IsChecked;
                    //    gesture.TrackZAxis = (bool)PreRecordingControl.TrackZCheckbox.IsChecked;
                    //    gesture.SaveFile(dialog.FileName);
                    //    RecordingControl.GestureFileName = dialog.FileName;
                    //    EditControl.LoadGesture(dialog.FileName);
                    //    MoveTo(EditPage);
                    //}
                    break;
                case "TestGesture" :
                    TestControl.TopLabel = "Test your gesture" ;
                    TestControl.LoadGesture(e.Data.ToString());
                    MoveTo(TestPage);
                    Testing = true;
                    break;
            }   
        }

        public string GestureName { get; set; }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(GesturePakFolder))
                Directory.CreateDirectory(GesturePakFolder);

            OriginalHeight = this.ActualHeight;
            OriginalWidth = this.ActualWidth;

            // Kinect Engine initialization
            KinectViewer = new ColorAndBodyViewer();
            KinectViewer.BodyTracked += KinectViewer_BodyTracked;

            KinectViewer.ShowLiveVideo = true;
            KinectViewer.DrawBodies = true;
            KinectViewer.JointThickness = 12;
            KinectViewer.TrackedJointBrush = new SolidColorBrush(Colors.DarkGreen);
            KinectViewer.TrackedBonePen = new Pen(Brushes.Green, 12);
            KinectViewer.FramePrefix = "frame";

            //listener = new SpeechTools.SpeechListener();
            //listener.RecognitionEngine.SetInputToDefaultAudioDevice();
            //listener.Phrases = MainWindowPhrases;
            //listener.SpeechRecognized += listener_SpeechRecognized;
            //listener.StartListening();

            // Create the gesture recorder
            Recorder = new GestureRecorder();

            // Show the first page
            if (UIPages != null && UIPages.Count > 0 && UIPages[0].UserControl != null)
            {
                NotifyPropertyChanged("SelectedPage");
                OnTransitionComplete(null, HomeControl);
                this.CurrentPage = HomePage;
            }
        }

        void listener_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            switch (e.Result.Text)
            {
                case "Record" :
                    MoveTo(PreRecordPage);
                    break;
                case "Load":
                    LoadGesture();
                    break;
                case "Interact" :
                    TestControl.TopLabel = "Play time!";
                    MoveTo(TestPage);
                    Testing = false;
                    break;
            }
        }


        private void MoveTo(NavPageInfo page)
        {

            if (page == null) return;
            
            var index = UIPages.IndexOf(page);

            if (index < 0 || index > UIPages.Count - 1 || index == pageIndex) return;

            this.LastPage = this.CurrentPage;

            if (page.UserControl == HomeControl)
            {
                //listener.RecognitionEngine.LoadGrammar(MainWindowGrammar);
            }
            
            if (index < pageIndex)
            {
                selectAndFlyOut = index;
                FlyOffRight.Begin();
            }
            else if (index > pageIndex)
            {
                selectAndFlyIn = index;
                FlyOffLeft.Begin();
            }
            this.CurrentPage = page;
        }

        public UserControl SelectedPage
        {
            get
            {
                if (pageIndex <= UIPages.Count - 1)
                {
                    return UIPages[pageIndex].UserControl;
                }
                else
                {
                    return null;
                }
            }

        }


        void FlyOffRight_Completed(object sender, EventArgs e)
        {
            if (selectAndFlyOut != -1)
            {
                pageIndex = selectAndFlyOut;
                selectAndFlyOut = -1;
                NotifyPropertyChanged("SelectedPage");
                FlyOut();
                OnTransitionComplete(null, UIPages[pageIndex].UserControl);
            }
        }

        void FlyOffLeft_Completed(object sender, EventArgs e)
        {
            if (selectAndFlyIn != -1)
            {
                pageIndex = selectAndFlyIn;
                selectAndFlyIn = -1;
                NotifyPropertyChanged("SelectedPage");
                FlyIn();
                OnTransitionComplete(null, UIPages[pageIndex].UserControl);
            }
        }

        void FlyIn()
        {
            AnimateRightToLeft.Begin();
        }

        void FlyOut()
        {
            AnimateLeftToRight.Begin();
        }


        void UserControl_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            
        }


        void KinectViewer_BodyTracked(object sender, BodyTrackedEventArgs e)
        {
            if (e.Body != null)
                Console.WriteLine("Got a body");

            if (Recording == true && Recorder != null)
            {
                Recorder.RecordFrame(e.Body);
            }

            if (Testing == true)
            {
                if (TestControl.Test(e.Body))
                {
                    TestControl.BigMessage = "Match";
                }
            }
        }

        public void OnTransitionComplete(UserControl LastControl, UserControl CurrentControl)
        {
            //if (this.LastPage != null)
            //{
            //    SmartUserControl last = (SmartUserControl)this.LastPage;
            //    //last.TransitionComplete();
            //}

            // remove existing event handlers
            if (KinectHappyUserControl != null)
            {
                KinectHappyUserControl = null;
            }

            // turn off the DataContext in all of the controls
            foreach (var UIPage in UIPages)
            {
                UIPage.UserControl.DataContext = null;
            }

            KinectHappyUserControl = (SmartUserControl)CurrentControl;

            this.DataContext = KinectHappyUserControl;

            if (KinectHappyUserControl == RecordingControl)
            {
                HookUpHappyControl(RecordingControl);
                RecordingControl.Start();
            }
            else if (KinectHappyUserControl == TestControl)
                HookUpHappyControl(TestControl);

        }

        void HookUpHappyControl(SmartUserControl uc)
        {
            KinectHappyUserControl = uc;
            KinectHappyUserControl.DataContext = KinectViewer;
        }

        // Clean up code

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (KinectViewer != null)
                KinectViewer.Dispose();
        }

        #region ScaleValue Depdency Property
        public static readonly DependencyProperty ScaleValueProperty = DependencyProperty.Register("ScaleValue", typeof(double), typeof(MainWindow), new UIPropertyMetadata(1.0, new PropertyChangedCallback(OnScaleValueChanged), new CoerceValueCallback(OnCoerceScaleValue)));

        private static object OnCoerceScaleValue(DependencyObject o, object value)
        {
            MainWindow mainWindow = o as MainWindow;
            if (mainWindow != null)
                return mainWindow.OnCoerceScaleValue((double)value);
            else
                return value;
        }

        private static void OnScaleValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            MainWindow mainWindow = o as MainWindow;
            if (mainWindow != null)
                mainWindow.OnScaleValueChanged((double)e.OldValue, (double)e.NewValue);
        }

        protected virtual double OnCoerceScaleValue(double value)
        {
            if (double.IsNaN(value))
                return 1.0f;

            value = Math.Max(0.1, value);
            return value;
        }

        protected virtual void OnScaleValueChanged(double oldValue, double newValue)
        {

        }

        public double ScaleValue
        {
            get
            {
                return (double)GetValue(ScaleValueProperty);
            }
            set
            {
                SetValue(ScaleValueProperty, value);
            }
        }
        #endregion

        private void CalculateScale()
        {
            if (OriginalHeight == 0.0f) return;
            double yScale = ActualHeight / OriginalHeight;
            double xScale = ActualWidth / OriginalWidth;
            double value = Math.Min(xScale, yScale);
            ScaleValue = (double)OnCoerceScaleValue(Root, value);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            //masterContent.Height = this.ActualHeight - 120;
        }

        private void MainGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CalculateScale();
        }

        private void _OnSystemCommandCloseWindow(object sender, ExecutedRoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
