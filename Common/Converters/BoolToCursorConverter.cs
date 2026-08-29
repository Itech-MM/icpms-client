using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;

namespace icpms_client.Common.Converters;

public class BoolToCursorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? Cursors.Hand : Cursors.Arrow;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}