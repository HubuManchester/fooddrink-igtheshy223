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

        // no network return null show bottom error placeholder
        if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            return null;

        // have network show image
        if (path.StartsWith("http"))
            return ImageSource.FromUri(new Uri(path));

        /* local pack resource */
        return ImageSource.FromFile(path);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}
