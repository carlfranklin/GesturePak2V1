using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using GesturePak;

namespace GesturePakRecorder
{
    public class TimeSpanMSToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                return ((TimeSpan)value).TotalMilliseconds.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                var ms = System.Convert.ToSingle(value);
                return TimeSpan.FromMilliseconds(ms);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
