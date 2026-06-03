namespace ssk.Models;

public class MealGroup
{
    public string MealType { get; set; } = string.Empty;
    public List<MealPlan> Plans { get; set; } = new();
    public string Icon => MealType switch
    {
        "Breakfast" => "",    // fa-coffee
        "Lunch" => "",    // fa-cutlery
        "Dinner" => "",    // fa-moon-o
        "Snack" => "",    // fa-apple-alt
        _ => ""
    };

    public MealGroup() { }

    public MealGroup(string mealType, List<MealPlan> plans)
    {
        MealType = mealType;
        Plans = plans;
    }
}
