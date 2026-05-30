namespace ssk.Helpers;

public static class AdaptiveHelper
{
    public static bool IsTablet =>
        DeviceInfo.Idiom == DeviceIdiom.Tablet ||
        DeviceInfo.Idiom == DeviceIdiom.Desktop;

    public static int ComputeSpan(double width, double itemWidth = 180)
        => Math.Max(2, (int)(width / itemWidth));
}
