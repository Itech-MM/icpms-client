using System.Windows;

namespace icpms_client.Common.Converters;

using System;
using System.Globalization;
using System.Windows.Data;
using FontAwesome.WPF;

public class WindowStateToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is WindowState state)
        {
            return state == WindowState.Maximized 
                ? FontAwesomeIcon.WindowRestore 
                : FontAwesomeIcon.WindowMaximize;
        }
        return FontAwesomeIcon.WindowMaximize; // Default
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}