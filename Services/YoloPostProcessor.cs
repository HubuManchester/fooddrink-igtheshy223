using ssk.Models;
using Microsoft.Maui.Graphics;

namespace ssk.Services;

public static class YoloPostProcessor
{
    public static List<YoloPrediction> ProcessOutput(float[] output, int originalWidth, int originalHeight,
        float confidenceThreshold = 0.5f, float iouThreshold = 0.45f)
    {
        var predictions = new List<YoloPrediction>();

        if (output == null || output.Length == 0)
            return predictions;

        // YOLOv8 输出 shape: [1, 4+numClasses, numDetections]
        // 按前两个维度的乘积反推 numDetections 和 numClasses
        int numClasses = (output.Length / 8400) - 4;
        if (numClasses <= 0)
            return predictions;

        int numDetections = output.Length / (4 + numClasses);

        // 获取 letterbox 反算参数
        var (scale, offsetX, offsetY) =
            SkiaImagePreprocessor.ComputeLetterbox(originalWidth, originalHeight, 640);

        for (int p = 0; p < numDetections; p++)
        {
            // 中心坐标和宽高（letterbox 空间）
            float cx = output[0 * numDetections + p];
            float cy = output[1 * numDetections + p];
            float w  = output[2 * numDetections + p];
            float h  = output[3 * numDetections + p];

            // 找最高类别分数
            int bestClass = 0;
            float bestScore = 0;
            for (int c = 0; c < numClasses; c++)
            {
                float score = output[(4 + c) * numDetections + p];
                if (score > bestScore)
                {
                    bestScore = score;
                    bestClass = c;
                }
            }

            if (bestScore < confidenceThreshold)
                continue;

            // 从 letterbox 空间还原到原图像素坐标
            float x1 = Math.Clamp(((cx - w / 2) - offsetX) / scale, 0, originalWidth);
            float y1 = Math.Clamp(((cy - h / 2) - offsetY) / scale, 0, originalHeight);
            float x2 = Math.Clamp(((cx + w / 2) - offsetX) / scale, 0, originalWidth);
            float y2 = Math.Clamp(((cy + h / 2) - offsetY) / scale, 0, originalHeight);

            predictions.Add(new YoloPrediction(
                GetLabelForClass(bestClass),
                bestScore,
                new RectF(x1, y1, x2 - x1, y2 - y1)));
        }

        return ApplyNms(predictions, iouThreshold);
    }

    /// <summary>
    /// COCO 80 类数据集的类别 ID → 标签名映射。
    /// 如果使用自定义训练模型，需要更新此映射。
    /// </summary>
    private static string GetLabelForClass(int classId) => classId switch
    {
        46 => "banana", 47 => "apple", 49 => "orange",
        53 => "pizza", 55 => "cake", 52 => "hot dog",
        54 => "sandwich", 50 => "broccoli", 48 => "carrot",
        45 => "bowl", 63 => "dining table", 72 => "refrigerator",
        41 => "cup", 39 => "bottle", 40 => "wine glass",
        42 => "fork", 43 => "knife", 44 => "spoon",
        69 => "microwave", 70 => "oven", 80 => "toaster",
        71 => "sink",
        _ => $"class_{classId}"
    };

    private static List<YoloPrediction> ApplyNms(List<YoloPrediction> predictions, float iouThreshold)
    {
        var result = new List<YoloPrediction>();
        var sorted = predictions.OrderByDescending(p => p.Confidence).ToList();

        while (sorted.Count > 0)
        {
            var best = sorted[0];
            result.Add(best);
            sorted.RemoveAt(0);
            sorted.RemoveAll(p => CalculateIoU(best.BoundingBox, p.BoundingBox) > iouThreshold);
        }

        return result;
    }

    private static float CalculateIoU(RectF box1, RectF box2)
    {
        var x1 = Math.Max(box1.Left, box2.Left);
        var y1 = Math.Max(box1.Top, box2.Top);
        var x2 = Math.Min(box1.Right, box2.Right);
        var y2 = Math.Min(box1.Bottom, box2.Bottom);

        var intersection = Math.Max(0, x2 - x1) * Math.Max(0, y2 - y1);
        var area1 = box1.Width * box1.Height;
        var area2 = box2.Width * box2.Height;
        var union = area1 + area2 - intersection;

        return union > 0 ? intersection / union : 0;
    }
}
