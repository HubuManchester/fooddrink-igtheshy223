using SQLite;
namespace ssk.Models;

[Table("ingredients")]
public class Ingredient
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int UserId { get; set; }

    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Category { get; set; } = "蔬菜";

    public string? ImagePath { get; set; }

    [MaxLength(20)]
    public string DefaultUnit { get; set; } = "克";

    public double Quantity { get; set; }

    [Ignore]
    public string QuantityDisplay => $"{Quantity:F0} {DefaultUnit}";

    public DateTime? PurchaseDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    [MaxLength(10)]
    public string StorageLocation { get; set; } = "冰箱";

    public string? NutritionPer100g { get; set; }

    [MaxLength(50)]
    public string? YoloLabel { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public Ingredient() { }

    public Ingredient(string name, string category, string? imagePath, string defaultUnit,
        double quantity, DateTime? purchaseDate, DateTime? expiryDate,
        string storageLocation, string? nutritionPer100g, string? yoloLabel)
    {
        Name = name;
        Category = category;
        ImagePath = imagePath;
        DefaultUnit = defaultUnit;
        Quantity = quantity;
        PurchaseDate = purchaseDate;
        ExpiryDate = expiryDate;
        StorageLocation = storageLocation;
        NutritionPer100g = nutritionPer100g;
        YoloLabel = yoloLabel;
    }
}
