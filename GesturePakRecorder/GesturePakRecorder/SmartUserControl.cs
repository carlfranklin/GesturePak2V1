using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace GesturePakRecorder
{
    public delegate void DataCompleteEventHandler(object sender, DataCompleteEventArgs e);

    public class SmartUserControl : UserControl, INotifyPropertyChanged
    {
        public event DataCompleteEventHandler DataComplete;

        public virtual void OnDataComplete(DataCompleteEventArgs e)
        {
            if (DataComplete != null)
            {
                DataComplete(this, e);
            }
        }

        bool enableNextButton = true;
        public bool EnableNextButton 
        {
            get { return enableNextButton; }
            set
            {
                enableNextButton = value;
                NotifyPropertyChanged();
            }
        }

        public virtual void KinectViewer_BodyTracked(object sender, BodyTrackedEventArgs e)
        {

        }

        public virtual void TransitionComplete()
        {

        }

        public event EventHandler TestStarted;
        public virtual void OnTestStarted(EventArgs args)
        {
            if (TestStarted != null)
                TestStarted(this, args);
        }

        public event EventHandler TestCompleted;
        public virtual void OnTestCompleted(EventArgs args)
        {
            if (TestCompleted != null)
                TestCompleted(this, args);
        }

        public event EventHandler TransitionCompleted;
        public virtual void OnTransitionCompleted(EventArgs args)
        {
            if (TransitionCompleted != null)
                TransitionCompleted(this, args);
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
