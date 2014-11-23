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
    /// Interaction logic for UCPreRecording.xaml
    /// </summary>
    public partial class UCPreRecording : SmartUserControl
    {
        public UCPreRecording()
        {
            InitializeComponent();
            EnableNextButton = true;
            this.Loaded += UCPreRecording_Loaded;
        }

        void UCPreRecording_Loaded(object sender, RoutedEventArgs e)
        {
            GestureNameTextBox.Focus();
        }

        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            OnDataComplete(new DataCompleteEventArgs("RecordPage", null));
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            OnDataComplete(new DataCompleteEventArgs("CancelRecord", null));
        }
    }
}
