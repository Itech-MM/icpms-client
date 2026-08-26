using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace icpms_client.UserControls.UI.Reusable.Utils
{
    class LighterBrushConverter : IValueConverter
    {
        object IValueConverter.Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
                return Brushes.Transparent;
            
            if (value is SolidColorBrush brush)
            {
                var color = brush.Color;
                return new SolidColorBrush(Color.FromArgb(
                    (byte)(color.A * 0.5),
                    (byte)Math.Min(225, color.R + 10),
                    (byte)Math.Min(225, color.G + 10),
                    (byte)Math.Min(225, color.B + 10)
                    ));
            }
            return value;
        }

        object IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public SolidColorBrush Convert(Color brush)
        {
            return new SolidColorBrush(Color.FromArgb(
                (byte)(brush.A * 0.5),
                (byte)Math.Min(225, brush.R + 10),
                (byte)Math.Min(225, brush.G + 10),
                (byte)Math.Min(225, brush.B + 10)
            ));
        }
    }
}
