using SQLite;
namespace ssk.Models;

[Table("meal_plans")]
public class MealPlan
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int UserId { get; set; }

    [MaxLength(10)]
    public string PlanDate { get; set; } = DateTime.Today.ToString("yyyy-MM-dd");

    [MaxLength(10)]
    public string MealType { get; set; } = "早餐";

    public int? RecipeId { get; set; }

    [MaxLength(100)]
    public string? CustomFoodName { get; set; }

    public double Servings { get; set; } = 1;

    public double Calories { get; set; }
    public double Protein { get; set; }
    public double Carbs { get; set; }
    public double Fat { get; set; }
    public double Fiber { get; set; }

    public string? Notes { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public MealPlan() { }

    public MealPlan(string planDate, string mealType, int? recipeId,
        string? customFoodName, double servings, double calories, double protein,
        double carbs, double fat, double fiber, string? notes, bool isCompleted)
    {
        PlanDate = planDate;
        MealType = mealType;
        RecipeId = recipeId;
        CustomFoodName = customFoodName;
        Servings = servings;
        Calories = calories;
        Protein = protein;
        Carbs = carbs;
        Fat = fat;
        Fiber = fiber;
        Notes = notes;
        IsCompleted = isCompleted;
    }
}
