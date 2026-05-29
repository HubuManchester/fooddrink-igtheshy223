using SQLite;
namespace ssk.Models;

[Table("nutrition_targets")]
public class NutritionTarget
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int UserId { get; set; }

    // Uniqueness is enforced in the repository, not via attribute
    [MaxLength(10)]
    public string TargetDate { get; set; } = DateTime.Today.ToString("yyyy-MM-dd");

    public double CaloriesGoal { get; set; } = 2000;
    public double ProteinGoal { get; set; } = 60;
    public double CarbsGoal { get; set; } = 250;
    public double FatGoal { get; set; } = 65;
    public double FiberGoal { get; set; } = 25;

    public NutritionTarget() { }

    public NutritionTarget(string targetDate, double caloriesGoal, double proteinGoal,
        double carbsGoal, double fatGoal, double fiberGoal)
    {
        TargetDate = targetDate;
        CaloriesGoal = caloriesGoal;
        ProteinGoal = proteinGoal;
        CarbsGoal = carbsGoal;
        FatGoal = fatGoal;
        FiberGoal = fiberGoal;
    }
}
