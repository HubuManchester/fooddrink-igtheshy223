using System.Globalization;
using Microsoft.Maui.Controls;

namespace ssk.Converters;

public class NutritionScoreConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double score)
        {
            return score switch
            {
                >= 80 => "优秀",
                >= 60 => "良好",
                >= 40 => "一般",
                _ => "需改善"
            };
        }
        return "未知";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => 0.0;
}
