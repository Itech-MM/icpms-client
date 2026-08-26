using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using icpms_client.ViewModels.Wrappers;

namespace icpms_client.Utils.Converters;

public class LabelValueWrapperConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return new ObservableCollection<LabelValueWrapper>();

        if (value is IEnumerable enumerable and not string)
        {
            var result = new ObservableCollection<LabelValueWrapper>();
            foreach (var item in enumerable)
            {
                result.Add(CreateLabelValueWrapper(item));
            }
            return result;
        }

        if (targetType == typeof(ObservableCollection<LabelValueWrapper>) || 
            typeof(IEnumerable).IsAssignableFrom(targetType))
        {
            return new ObservableCollection<LabelValueWrapper> { CreateLabelValueWrapper(value) };
        }

        return targetType == typeof(LabelValueWrapper) ? CreateLabelValueWrapper(value) : null;
    }

    private static LabelValueWrapper CreateLabelValueWrapper(object? item)
    {
        string stringValue = item switch
        {
            string s => s,
            null => string.Empty,
            _ => item.ToString() ?? string.Empty
        };
        return new LabelValueWrapper(stringValue, stringValue);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is LabelValueWrapper wrapper)
        {
            return wrapper.Value;
        }

        return string.Empty;
    }
}