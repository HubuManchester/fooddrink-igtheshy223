using System.Globalization;
using Microsoft.Maui.Controls;

namespace ssk.Converters;

public class IntToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is int i && i != 0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => 0;
}
