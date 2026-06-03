using SQLite;
namespace ssk.Models;

[Table("recipes")]
public class Recipe
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ImagePath { get; set; }

    [MaxLength(20)]
    public string Category { get; set; } = "Chinese";

    public int PrepTimeMin { get; set; }

    public int CookTimeMin { get; set; }

    public int Servings { get; set; } = 1;

    [MaxLength(10)]
    public string Difficulty { get; set; } = "Easy";

    public string Instructions { get; set; } = "[]";

    public string? Tags { get; set; }

    public bool IsFavorite { get; set; }

    public bool IsCustom { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public Recipe() { }

    public Recipe(string title, string? description, string? imagePath, string category,
        int prepTimeMin, int cookTimeMin, int servings, string difficulty,
        string instructions, string? tags, bool isFavorite, bool isCustom)
    {
        Title = title;
        Description = description;
        ImagePath = imagePath;
        Category = category;
        PrepTimeMin = prepTimeMin;
        CookTimeMin = cookTimeMin;
        Servings = servings;
        Difficulty = difficulty;
        Instructions = instructions;
        Tags = tags;
        IsFavorite = isFavorite;
        IsCustom = isCustom;
    }
}
