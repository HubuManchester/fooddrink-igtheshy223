using Microsoft.Maui.Graphics;

namespace ssk.Models;

public class YoloPrediction
{
    public string Label { get; set; } = string.Empty;
    public float Confidence { get; set; }
    public RectF BoundingBox { get; set; }

    public YoloPrediction() { }

    public YoloPrediction(string label, float confidence, RectF boundingBox)
    {
        Label = label;
        Confidence = confidence;
        BoundingBox = boundingBox;
    }
}
