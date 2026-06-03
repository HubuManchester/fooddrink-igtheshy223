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
                >= 80 => "Excellent",
                >= 60 => "Good",
                >= 40 => "Average",
                _ => "Need Improve"
            };
        }
        return "Unknown";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => 0.0;
}
