namespace ssk.Services;

public class HapticService
{
    public async Task SuccessAsync()
    {
        try
        {
            await DefaultHapticAsync(100);
        }
        catch { }
    }

    public async Task ErrorAsync()
    {
        try
        {
            await DefaultHapticAsync(50);
            await Task.Delay(100);
            await DefaultHapticAsync(50);
        }
        catch { }
    }

    public async Task LightAsync()
    {
        try
        {
            await DefaultHapticAsync(50);
        }
        catch { }
    }

    private static async Task DefaultHapticAsync(int duration)
    {
#if ANDROID
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            try
            {
                var context = Platform.CurrentActivity ?? global::Android.App.Application.Context;
                var vibrator = (global::Android.OS.Vibrator?)context.GetSystemService(global::Android.Content.Context.VibratorService);
                if (vibrator == null) return;
#pragma warning disable CA1416
                if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.O)
                {
                    vibrator.Vibrate(global::Android.OS.VibrationEffect.CreateOneShot(duration, global::Android.OS.VibrationEffect.DefaultAmplitude));
                }
                else
                {
                    vibrator.Vibrate(duration);
                }
#pragma warning restore CA1416
            }
            catch { }
        });
#endif
        await Task.CompletedTask;
    }
}
