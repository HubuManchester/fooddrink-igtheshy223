using ssk.Models;

namespace ssk.Services;

public static class NutritionCalculator
{
    public static NutritionInfo CalculateForRecipe(Recipe recipe, double servings = 1)
    {
        // Basic estimation (scaled by servings)
        var factor = servings / Math.Max(recipe.Servings, 1);
        return new NutritionInfo
        {
            Calories = Math.Round(350 * factor, 1),
            Protein = Math.Round(15 * factor, 1),
            Carbs = Math.Round(40 * factor, 1),
            Fat = Math.Round(12 * factor, 1),
            Fiber = Math.Round(5 * factor, 1)
        };
    }

    public static NutritionInfo CalculateForIngredient(Ingredient ingredient)
    {
        if (string.IsNullOrEmpty(ingredient.NutritionPer100g))
            return NutritionInfo.Empty;
        try
        {
            var json = ingredient.NutritionPer100g;
            var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, double>>(json);
            if (dict == null) return NutritionInfo.Empty;
            var factor = ingredient.Quantity / 100.0;
            return new NutritionInfo
            {
                Calories = Math.Round(dict.GetValueOrDefault("calories", 0) * factor, 1),
                Protein = Math.Round(dict.GetValueOrDefault("protein", 0) * factor, 1),
                Carbs = Math.Round(dict.GetValueOrDefault("carbs", 0) * factor, 1),
                Fat = Math.Round(dict.GetValueOrDefault("fat", 0) * factor, 1),
                Fiber = Math.Round(dict.GetValueOrDefault("fiber", 0) * factor, 1)
            };
        }
        catch { return NutritionInfo.Empty; }
    }

    public static List<string> GenerateAdvice(DailyNutritionSummary summary)
    {
        var advice = new List<string>();
        var calPercent = summary.CalorieGoal > 0 ? summary.TotalCalories / summary.CalorieGoal * 100 : 0;

        if (calPercent < 50)
            advice.Add("Today calorie intake too low, suggest add more nutrition");
        else if (calPercent > 120)
            advice.Add("Today calorie intake too high, please control diet");
        else
            advice.Add("Today calorie intake is good, keep it up");

        if (summary.TotalProtein < summary.ProteinGoal * 0.8)
            advice.Add("Protein intake not enough, suggest add Meat, Dairy or Tofu");
        if (summary.TotalCarbs > summary.CarbsGoal * 1.2)
            advice.Add("Carbs intake too high, suggest reduce refined carbohydrate");
        if (summary.TotalFat > summary.FatGoal * 1.2)
            advice.Add("Fat intake too high, suggest choose low fat food");
        if (summary.TotalFiber < summary.FiberGoal * 0.6)
            advice.Add("Dietary fiber not enough, suggest eat more Vegetable and Fruit");

        if (advice.Count == 1)
            advice.Add("Overall nutrition balanced well, please keep it up");

        return advice;
    }
}
