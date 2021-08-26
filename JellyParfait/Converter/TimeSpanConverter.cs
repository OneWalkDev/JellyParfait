using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace JellyParfait.Converter
{
    [ValueConversion(typeof(TimeSpan), typeof(string))]
    public class TimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var time = (TimeSpan)value;
            if(time.Hours > 0)
            {
                return time.ToString(@"hh\:mm\:ss");
            }
            else
            {
                return time.ToString(@"mm\:ss");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return TimeSpan.Parse(value as string);
        }
    }
}
