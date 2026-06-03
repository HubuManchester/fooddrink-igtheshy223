namespace ssk.Models;

public class DailyNutritionSummary
{
    public string Date { get; set; } = string.Empty;

    // already eat only count IsCompleted equal true
    public double TotalCalories { get; set; }
    public double TotalProtein { get; set; }
    public double TotalCarbs { get; set; }
    public double TotalFat { get; set; }
    public double TotalFiber { get; set; }

    /* plan count all of them */
    public double PlannedCalories { get; set; }
    public double PlannedProtein { get; set; }
    public double PlannedCarbs { get; set; }
    public double PlannedFat { get; set; }
    public double PlannedFiber { get; set; }

    // target value
    public double CalorieGoal { get; set; } = 2000;
    public double ProteinGoal { get; set; } = 60;
    public double CarbsGoal { get; set; } = 250;
    public double FatGoal { get; set; } = 65;
    public double FiberGoal { get; set; } = 25;

    public DailyNutritionSummary() { }

    public DailyNutritionSummary(string date,
        double totalCalories, double totalProtein, double totalCarbs, double totalFat, double totalFiber,
        double plannedCalories, double plannedProtein, double plannedCarbs, double plannedFat, double plannedFiber,
        double calorieGoal, double proteinGoal, double carbsGoal, double fatGoal, double fiberGoal)
    {
        Date = date;
        TotalCalories = totalCalories;
        TotalProtein = totalProtein;
        TotalCarbs = totalCarbs;
        TotalFat = totalFat;
        TotalFiber = totalFiber;
        PlannedCalories = plannedCalories;
        PlannedProtein = plannedProtein;
        PlannedCarbs = plannedCarbs;
        PlannedFat = plannedFat;
        PlannedFiber = plannedFiber;
        CalorieGoal = calorieGoal;
        ProteinGoal = proteinGoal;
        CarbsGoal = carbsGoal;
        FatGoal = fatGoal;
        FiberGoal = fiberGoal;
    }
}
