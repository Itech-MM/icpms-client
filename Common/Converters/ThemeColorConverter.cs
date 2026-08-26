using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace icpms_client.Common.Converters;

public class ThemeColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (value is bool isDark && isDark) 
            ? new SolidColorBrush(Colors.White) 
            : new SolidColorBrush(Colors.Black);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}