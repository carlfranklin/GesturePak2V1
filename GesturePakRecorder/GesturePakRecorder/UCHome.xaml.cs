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

namespace GesturePakRecorder
{
    /// <summary>
    /// Interaction logic for UCHome.xaml
    /// </summary>
    public partial class UCHome : SmartUserControl
    {
        public UCHome()
        {
            InitializeComponent();
            EnableNextButton = false;
            RecordButton.Click += RecordButton_Click;
            LoadButton.Click += LoadButton_Click;
            InteractButton.Click += InteractButton_Click;
        }

        void InteractButton_Click(object sender, RoutedEventArgs e)
        {
            var args = new DataCompleteEventArgs("Interact", null);
            OnDataComplete(args);
        }

        void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var args = new DataCompleteEventArgs("Load", null);
            OnDataComplete(args);
        }

        void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            var args = new DataCompleteEventArgs("Pre-Record", null);
            OnDataComplete(args);
        }
    }
}
