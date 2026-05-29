using SQLite;
namespace ssk.Models;

[Table("shopping_items")]
public class ShoppingItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int UserId { get; set; }

    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Category { get; set; } = "蔬菜";

    public double Quantity { get; set; }

    [MaxLength(20)]
    public string Unit { get; set; } = "克";

    [Ignore]
    public string QuantityDisplay => $"{Quantity:F0} {Unit}";

    public bool IsPurchased { get; set; }

    [MaxLength(20)]
    public string? Source { get; set; }

    public int? SourceRecipeId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ShoppingItem() { }

    public ShoppingItem(string name, string category, double quantity,
        string unit, bool isPurchased, string? source, int? sourceRecipeId)
    {
        Name = name;
        Category = category;
        Quantity = quantity;
        Unit = unit;
        IsPurchased = isPurchased;
        Source = source;
        SourceRecipeId = sourceRecipeId;
    }
}
