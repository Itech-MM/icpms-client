using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace icpms_client.Utils.Converters;

public class CollectionEmptyConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is ICollection { Count: 0 };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}