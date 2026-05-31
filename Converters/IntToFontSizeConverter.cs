using System.Globalization;
using Microsoft.Maui.Controls;

namespace ssk.Converters;

public class IntToFontSizeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is int size ? (double)size : 14.0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => 14;
}
