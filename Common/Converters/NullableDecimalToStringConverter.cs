using System.Globalization;
using System.Windows.Data;

namespace icpms_client.Common.Converters;

public class NullableDecimalToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is decimal d ? d.ToString(CultureInfo.InvariantCulture) : string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var text = value as string;
        Console.WriteLine($"[Converter] ConvertBack called with: '{text}'");
        if (string.IsNullOrWhiteSpace(text) || text == ".") return null;
        var success = decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var result);
        Console.WriteLine($"[Converter] TryParse success={success}, result={result}");
        return success ? (decimal?)result : Binding.DoNothing;
    }
}