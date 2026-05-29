namespace ssk.Models;

public class MealGroup
{
    public string MealType { get; set; } = string.Empty;
    public List<MealPlan> Plans { get; set; } = new();
    public string Icon => MealType switch
    {
        "早餐" => "",    // fa-coffee
        "午餐" => "",    // fa-cutlery
        "晚餐" => "",    // fa-moon-o
        "加餐" => "",    // fa-apple-alt
        _ => ""
    };

    public MealGroup() { }

    public MealGroup(string mealType, List<MealPlan> plans)
    {
        MealType = mealType;
        Plans = plans;
    }
}
