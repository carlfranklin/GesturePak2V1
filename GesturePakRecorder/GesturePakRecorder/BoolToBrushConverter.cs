using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace GesturePakRecorder
{
    public class BoolToBrushConverter : IValueConverter
    {
        private Brush ButtonBrush;

        public BoolToBrushConverter()
        {
            ButtonBrush = new SolidColorBrush(Color.FromArgb(0xff, 0xdd, 0xdd, 0xdd));
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value == true)
                return System.Windows.Media.Brushes.LightGreen;
            else
                return ButtonBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
