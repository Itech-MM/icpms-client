using System;
using System.Globalization;
using System.Windows.Data;

namespace icpms_client.Utils.Converters
{
    public class WeightConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
                return 0;

            if (values[0] is double weight && values[1] is double perKgValue && perKgValue != 0)
            {
                Console.WriteLine($"Weight: {weight}, PerKgValue: {perKgValue}, Value: {weight * perKgValue}");
                return (weight * perKgValue).ToString("F2",CultureInfo.CurrentCulture);
            }

            return 0;
        }

        public object[]? ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}