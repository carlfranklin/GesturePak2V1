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
using System.Windows.Threading;

namespace GesturePakRecorder
{
    /// <summary>
    /// Interaction logic for UCRecorder.xaml
    /// </summary>
    public partial class UCRecorder : SmartUserControl
    {
        DispatcherTimer countDownTimer = new DispatcherTimer();
        int countdownSeconds = 5;
        bool countdownDone = false;
        DateTime startTime;

        public UCRecorder()
        {
            InitializeComponent();
            countDownTimer.Interval = TimeSpan.FromSeconds(1);
            countDownTimer.Tick += countDownTimer_Tick;
        }

        void countDownTimer_Tick(object sender, EventArgs e)
        {
            int elapsed = Convert.ToInt32(DateTime.Now.Subtract(startTime).TotalSeconds);

            if (elapsed >= countdownSeconds)
            {
                if (!countdownDone)
                {
                    countdownDone = true;
                    countdownSeconds = GestureSeconds;
                    startTime = DateTime.Now;
                    CountDown = "GO";
                    OnDataComplete(new DataCompleteEventArgs("StartRecording", null));
                }
                else
                {
                    CountDown = "";
                    countDownTimer.Stop();
                    OnDataComplete(new DataCompleteEventArgs("StopRecording", null));
                }
            }
            else
            {
                int secs = countdownSeconds - elapsed;
                CountDown = secs.ToString();
            }
        }

        public string GestureFileName { get; set; }

        public int GestureSeconds { get; set; }

        public void Start()
        {
            if (GestureSeconds != 0)
            {
                countdownDone = false;
                countdownSeconds = 10;
                startTime = DateTime.Now;
                CountDown = countdownSeconds.ToString();
                countDownTimer.Start();
            }
        }

        string countdown = "";
        public string CountDown
        {
            get
            {
                return countdown;
            }
            set
            {
                countdown = value;
                NotifyPropertyChanged();
            }
        }
    }
}
