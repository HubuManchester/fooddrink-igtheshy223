using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Networking;

namespace ssk.Converters;

public class StringToUriConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string path || string.IsNullOrEmpty(path))
            return null;

        // 无网络 → 返回 null → 显示底层错误占位符
        if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            return null;

        // 有网络 → 显示图片
        if (path.StartsWith("http"))
            return ImageSource.FromUri(new Uri(path));

        // 本地打包资源
        return ImageSource.FromFile(path);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}
