using ssk.Models;

namespace ssk.Services;

public static class NutritionCalculator
{
    public static NutritionInfo CalculateForRecipe(Recipe recipe, double servings = 1)
    {
        // 基础估算（基于份数缩放）
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
            advice.Add("今日摄入热量偏低，建议适当补充营养");
        else if (calPercent > 120)
            advice.Add("今日摄入热量偏高，注意控制饮食");
        else
            advice.Add("今日热量摄入良好，继续保持");

        if (summary.TotalProtein < summary.ProteinGoal * 0.8)
            advice.Add("蛋白质摄入不足，建议增加肉类、蛋奶或豆制品");
        if (summary.TotalCarbs > summary.CarbsGoal * 1.2)
            advice.Add("碳水摄入偏高，建议减少精制碳水化合物");
        if (summary.TotalFat > summary.FatGoal * 1.2)
            advice.Add("脂肪摄入偏高，建议选择低脂食物");
        if (summary.TotalFiber < summary.FiberGoal * 0.6)
            advice.Add("膳食纤维摄入不足，建议多吃蔬菜水果");

        if (advice.Count == 1)
            advice.Add("整体营养搭配合理，请继续保持");

        return advice;
    }
}
