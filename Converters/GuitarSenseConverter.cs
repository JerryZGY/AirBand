using System;
using System.Windows.Data;

namespace AirBand
{
    public class GuitarSenseConverter : IValueConverter
    {
        public object Convert (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return 100 - (int)value / 10;
        }

        public object ConvertBack (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return 1000 - ( (double)value * 10 );
        }
    }
}