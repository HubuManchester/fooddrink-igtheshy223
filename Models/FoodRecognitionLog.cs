using SQLite;
namespace ssk.Models;

[Table("food_recognition_logs")]
public class FoodRecognitionLog
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? ImagePath { get; set; }

    [MaxLength(50)]
    public string RecognizedLabel { get; set; } = string.Empty;

    public float Confidence { get; set; }

    [MaxLength(50)]
    public string? MappedFoodName { get; set; }

    public bool WasAddedToPlan { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public FoodRecognitionLog() { }

    public FoodRecognitionLog(string? imagePath, string recognizedLabel,
        float confidence, string? mappedFoodName, bool wasAddedToPlan)
    {
        ImagePath = imagePath;
        RecognizedLabel = recognizedLabel;
        Confidence = confidence;
        MappedFoodName = mappedFoodName;
        WasAddedToPlan = wasAddedToPlan;
    }
}
