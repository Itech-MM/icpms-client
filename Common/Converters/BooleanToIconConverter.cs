using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using FontAwesome.WPF;

namespace icpms_client.Common.Converters;

public class BooleanToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (value is bool isDark && isDark) 
            ? FontAwesomeIcon.MoonOutline 
            : FontAwesomeIcon.SunOutline;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

