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
    /// Interaction logic for UCTest.xaml
    /// </summary>
    public partial class UCTest : SmartUserControl
    {

        GestureMatcher Matcher = null;
        DispatcherTimer MatchTimer = new DispatcherTimer();

        public UCTest()
        {
            InitializeComponent();
            DoneButton.Click += DoneButton_Click;
            MatchTimer.Interval = TimeSpan.FromSeconds(1);
            MatchTimer.Tick += MatchTimer_Tick;
        }

        void MatchTimer_Tick(object sender, EventArgs e)
        {
            BigMessage = "";
        }

        public string GestureFileName { get; set; }

        string bigMessage = "";
        public string BigMessage
        {
            get
            {
                return bigMessage;
            }
            set
            {
                bigMessage = value;
                if (value.Length > 0)
                    MatchTimer.Start();
                else
                    MatchTimer.Stop();
                NotifyPropertyChanged();
            }
        }


        string topLabel = "Test your gesture";
        public string TopLabel
        {
            get
            {
                return topLabel;
            }
            set
            {
                topLabel = value;
                NotifyPropertyChanged();
            }
        }

        void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            OnDataComplete(new DataCompleteEventArgs("Back", null));
        }

        public void LoadGesture(string filename)
        {
            GestureFileName = filename;
            Matcher = new GestureMatcher(new List<Gesture>() { new Gesture(filename) });
        }

        public bool Test(Body body)
        {
            if (Matcher == null) return false;

            Matcher.Body = body;
            var gesture = Matcher.GetMatch();
            if (gesture != null)
                return true;
            else
                return false;
        }
    }
}
