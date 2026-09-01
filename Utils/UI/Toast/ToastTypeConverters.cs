using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using FontAwesome.WPF;

namespace icpms_client.Utils.UI.Toast;

public class ToastTypeToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var key = value switch
        {
            ToastType.Success => "SuccessBrush",
            ToastType.Error => "DangerBrush",
            ToastType.Warning => "WarningBrush",
            _ => "InfoBrush"
        };

        return Application.Current.TryFindResource(key) ?? Brushes.Gray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class ToastTypeToIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            ToastType.Success => FontAwesomeIcon.CheckCircle,
            ToastType.Error => FontAwesomeIcon.TimesCircle,
            ToastType.Warning => FontAwesomeIcon.ExclamationTriangle,
            _ => FontAwesomeIcon.InfoCircle
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class ToastTypeToBgConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var key = value switch
        {
            ToastType.Success => "SuccessChipBackgroundColor",
            ToastType.Error => "DangerChipBackgroundColor",
            ToastType.Warning => "WarningChipBackgroundColor",
            _ => "InfoChipBackgroundColor"
        };

        return Application.Current.TryFindResource(key) ?? Brushes.LightGray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}


public class ToastTypeToSurfaceBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string key = value switch
        {
            ToastType.Success => "SuccessChipBackgroundColor",
            ToastType.Error => "DangerChipBackgroundColor",
            ToastType.Warning => "WarningChipBackgroundColor",
            ToastType.Info => "InfoChipBackgroundColor",
            _ => "PrimaryChipBackgroundColor"
        };

        return Application.Current.TryFindResource(key)
               ?? Application.Current.FindResource("SurfaceBrush");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}