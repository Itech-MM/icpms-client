using System.Globalization;
using System.Windows.Data;

namespace icpms_client.Utils.Converters;

public class OverWeightConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length != 2)
            return 0;

        if (values[0] is double totalWeight && values[1] is double allowedWeight)
        {
            double overWeight = Math.Max(0, totalWeight - allowedWeight); // Ensure non-negative value
            Console.WriteLine($"TotalWeight: {totalWeight}, AllowedWeight: {allowedWeight}, OverWeight: {overWeight}");
            return overWeight.ToString("F2", CultureInfo.CurrentCulture);
        }

        return 0;
    }

    public object[]? ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return null;
    }
}