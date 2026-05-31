using System.Globalization;
using Microsoft.Maui.Controls;

namespace ssk.Converters;

public class ExpiryToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime expiryDate)
        {
            var days = (expiryDate - DateTime.Today).Days;
            return days switch
            {
                < 0 => $"已过期{-days}天",
                0 => "今天过期",
                <= 3 => $"{days}天后过期",
                _ => $"{days}天"
            };
        }
        return "";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => DateTime.MinValue;
}
