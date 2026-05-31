using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace ssk.Converters;

public class ExpiryToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime expiryDate)
        {
            var days = (expiryDate - DateTime.Today).Days;
            return days switch
            {
                < 0 => Color.FromArgb("#EF4444"),  // 红色 - 已过期
                <= 3 => Color.FromArgb("#F59E0B"),  // 橙色 - 即将过期
                _ => Color.FromArgb("#22C55E")      // 绿色 - 安全
            };
        }
        return Colors.Transparent;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => DateTime.MinValue;
}
