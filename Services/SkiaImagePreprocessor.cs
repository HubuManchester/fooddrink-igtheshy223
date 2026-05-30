using SkiaSharp;

namespace ssk.Services;

public static class SkiaImagePreprocessor
{
    public static async Task<float[]> PreprocessAsync(string imagePath, int targetSize = 640)
    {
        return await Task.Run(() =>
        {
            using var originalBitmap = SKBitmap.Decode(imagePath);
            if (originalBitmap == null)
                throw new InvalidOperationException($"无法解码图片: {imagePath}");

            var (scale, padX, padY) =
                ComputeLetterbox(originalBitmap.Width, originalBitmap.Height, targetSize);

            var scaledWidth = (int)(originalBitmap.Width * scale);
            var scaledHeight = (int)(originalBitmap.Height * scale);

            using var resizedBitmap = new SKBitmap(targetSize, targetSize);
            using var canvas = new SKCanvas(resizedBitmap);
            canvas.Clear(new SKColor(128, 128, 128)); // 灰色填充

            canvas.Save();
            canvas.Translate(padX, padY);
            canvas.Scale(
                (float)scaledWidth / originalBitmap.Width,
                (float)scaledHeight / originalBitmap.Height);
            canvas.DrawBitmap(originalBitmap, 0, 0);
            canvas.Restore();

            // 提取像素，转为 NCHW float tensor [1, 3, H, W]
            var result = new float[3 * targetSize * targetSize];
            var pixels = resizedBitmap.Pixels;
            var planeSize = targetSize * targetSize;

            for (int i = 0; i < planeSize; i++)
            {
                var pixel = pixels[i];
                result[0 * planeSize + i] = pixel.Red / 255f;
                result[1 * planeSize + i] = pixel.Green / 255f;
                result[2 * planeSize + i] = pixel.Blue / 255f;
            }

            return result;
        });
    }

    /// <summary>
    /// 计算 letterbox 变换参数，供 YoloPostProcessor 反算坐标时复用。
    /// </summary>
    public static (float scale, int offsetX, int offsetY) ComputeLetterbox(
        int originalWidth, int originalHeight, int targetSize)
    {
        var scale = Math.Min((float)targetSize / originalWidth, (float)targetSize / originalHeight);
        var scaledWidth = (int)(originalWidth * scale);
        var scaledHeight = (int)(originalHeight * scale);
        var offsetX = (targetSize - scaledWidth) / 2;
        var offsetY = (targetSize - scaledHeight) / 2;
        return (scale, offsetX, offsetY);
    }
}
