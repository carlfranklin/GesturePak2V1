using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GesturePakRecorder
{
    public class NavPageInfo
    {
        public SmartUserControl UserControl { get; set; }
        public bool Vertical { get; set; }
        public string About { get; set; }

        public NavPageInfo(SmartUserControl UserControl, bool Vertical, string About)
        {
            this.UserControl = UserControl;
            this.Vertical = Vertical;
            this.About = About;
        }
    }
}
