using System.Globalization;
using System.Windows.Data;

namespace icpms_client.Utils.UI.Dialog.MessageDialog;

public class MessageTypeConverter: IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is MessageType type)
        {
            return type.ToString();
        }
        return "Information";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}