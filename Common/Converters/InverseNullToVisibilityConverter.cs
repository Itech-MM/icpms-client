using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace icpms_client.Common.Converters
{
    public class InverseNullToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is string s && !string.IsNullOrEmpty(s)
                ? Visibility.Hidden
                : Visibility.Visible;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}