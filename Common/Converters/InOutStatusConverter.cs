using System;
using System.Globalization;
using System.Windows.Data;

namespace icpms_client.Common.Converters
{
    public class InOutStatusConverter : IValueConverter
    {
        // This method will convert the integer status into a corresponding string
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int status)
            {
                return status switch
                {
                    1 => "In Bound",
                    2 => "Out Bound",
                    3 => "Both",
                    _ => "Unknown" // Default case if the status is not 1, 2, or 3
                };
            }
            return "Unknown";
        }

        // This method will convert the string back to the corresponding integer value if needed
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return str switch
                {
                    "In Bound" => 1,
                    "Out Bound" => 2,
                    "Both" => 3,
                    _ => 0 // Default case if the string doesn't match
                };
            }
            return 0;
        }
    }
}