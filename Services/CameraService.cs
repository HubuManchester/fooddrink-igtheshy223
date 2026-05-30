namespace ssk.Services;

public class CameraService
{
    public async Task<FileResult?> CapturePhotoAsync()
    {
        try
        {
            var photo = await MediaPicker.CapturePhotoAsync(new MediaPickerOptions
            {
                Title = "食物照片"
            });
            return photo;
        }
        catch (FeatureNotSupportedException) { return null; }
        catch (PermissionException) { return null; }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Camera error: {ex.Message}");
            return null;
        }
    }

    public async Task<FileResult?> PickPhotoAsync()
    {
        try
        {
            return await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "选择食物照片"
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Pick photo error: {ex.Message}");
            return null;
        }
    }

    public async Task<string> SaveToLocalAsync(FileResult fileResult)
    {
        var localPath = Path.Combine(FileSystem.CacheDirectory, $"food_{DateTime.Now:yyyyMMddHHmmss}.jpg");
        using var stream = await fileResult.OpenReadAsync();
        using var fileStream = File.OpenWrite(localPath);
        await stream.CopyToAsync(fileStream);
        return localPath;
    }

#if ANDROID
    public Task<bool> ToggleFlashAsync(bool on)
    {
        try
        {
            var activity = Platform.CurrentActivity;
            if (activity == null) return Task.FromResult(false);

            var cameraManager = (Android.Hardware.Camera2.CameraManager?)
                activity.GetSystemService(Android.Content.Context.CameraService);
            if (cameraManager == null) return Task.FromResult(false);

            var cameraIds = cameraManager.GetCameraIdList();
            if (cameraIds == null || cameraIds.Length == 0) return Task.FromResult(false);

            // 使用后置摄像头（通常是第一个）
            foreach (var id in cameraIds)
            {
                var characteristics = cameraManager.GetCameraCharacteristics(id);
                var facingObj = characteristics.Get(Android.Hardware.Camera2.CameraCharacteristics.LensFacing);
                var facing = (Android.Hardware.Camera2.LensFacing)((Java.Lang.Integer)facingObj).IntValue();
                if (facing == Android.Hardware.Camera2.LensFacing.Back)
                {
                    cameraManager.SetTorchMode(id, on);
                    return Task.FromResult(true);
                }
            }

            // 未找到后置摄像头，尝试第一个
            cameraManager.SetTorchMode(cameraIds[0], on);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CameraService] ToggleFlash error: {ex.Message}");
            return Task.FromResult(false);
        }
    }
#else
    public Task<bool> ToggleFlashAsync(bool on) => Task.FromResult(false);
#endif
}
