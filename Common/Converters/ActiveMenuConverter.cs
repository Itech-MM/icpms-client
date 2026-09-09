using System.Globalization;
using System.Windows.Data;
using icpms_client.Common.Enums;

namespace icpms_client.Common.Converters;

public class ActiveMenuConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 2 && values[0] is AppPage tag && values[1] is AppPage active)
            return tag == active;
        return false;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}