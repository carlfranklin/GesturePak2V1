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
using KinectTools;
using GesturePak;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GestureTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        ColorAndBodyViewer KinectViewer = null;
        GestureMatcher[] Matchers = null;

        string GestureFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\gesturepak";

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize the MultiEngine
            KinectViewer = new ColorAndBodyViewer();
            KinectViewer.ShowLiveVideo = true;
            KinectViewer.DrawBodies = false;
            KinectViewer.BodyTracked += MultiEngine_BodyTracked;
            this.DataContext = KinectViewer;

            // Load the Gesture Files
            var Gestures = new List<Gesture>();
            var files = Directory.GetFiles(GestureFolder, "*.xml");
            if (files.Length == 0)
            {
                MessageBox.Show("No gesture files in " + GestureFolder);
                return;
            }
            // create gestures and add them to a list
            foreach (string file in files)
            {
                Gestures.Add(new Gesture(file));
            }
            
            // create 6 matchers, one for each Body
            Matchers = new GestureMatcher[6];
            for (int i = 0; i < 6; i++)
            {
                Matchers[i] = new GestureMatcher(Gestures);
            }

        }

        void MultiEngine_BodyTracked(object sender, BodyTrackedEventArgs e)
        {
            Matchers[e.BodyIndex].Body = e.Body;
            var MatchedGesture = Matchers[e.BodyIndex].GetMatch();
            if (MatchedGesture != null)
            {
                // We have a match
                BigMessage = MatchedGesture.Name;
            }
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            KinectViewer.Dispose();
        }

        string bigmessage = "";
        public string BigMessage
        {
            get
            {
                return bigmessage;
            }
            set
            {
                bigmessage = value;
                NotifyPropertyChanged();
            }
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
