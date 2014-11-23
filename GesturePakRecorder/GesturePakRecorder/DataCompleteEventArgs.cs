using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GesturePakRecorder
{
    public class DataCompleteEventArgs : EventArgs
    {
        public string Title { get; set; }
        public object Data { get; set; }

        public DataCompleteEventArgs(string Title, object Data)
        {
            this.Title = Title;
            this.Data = Data;
        }
    }
}
