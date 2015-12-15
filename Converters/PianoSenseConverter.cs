using System;
using System.Windows.Data;

namespace AirBand
{
    public class PianoSenseConverter : IValueConverter
    {
        public object Convert (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return 100 - (int)value / 30;
        }

        public object ConvertBack (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return 3000 - ( (double)value * 30 );
        }
    }
}