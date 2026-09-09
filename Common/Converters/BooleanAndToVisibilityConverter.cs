using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace icpms_client.Common.Converters;

public class BooleanAndToVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var allTrue = values.OfType<bool>().All(v => v);
        return allTrue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}