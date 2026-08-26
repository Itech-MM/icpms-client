using System;
using System.Globalization;
using System.Windows.Data;

namespace icpms_client.Common.Converters
{
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool and false; // Inverts the boolean value
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool and false; // Inverts the boolean value for two-way binding
        }
    }
}