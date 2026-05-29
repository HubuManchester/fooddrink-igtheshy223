namespace ssk.Models;

public class NutritionInfo
{
    public double Calories { get; set; }
    public double Protein { get; set; }
    public double Carbs { get; set; }
    public double Fat { get; set; }
    public double Fiber { get; set; }

    public static NutritionInfo Empty => new();

    public NutritionInfo() { }

    public NutritionInfo(double calories, double protein, double carbs, double fat, double fiber)
    {
        Calories = calories;
        Protein = protein;
        Carbs = carbs;
        Fat = fat;
        Fiber = fiber;
    }
}
