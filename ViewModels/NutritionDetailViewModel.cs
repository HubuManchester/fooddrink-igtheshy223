using System.Collections.ObjectModel;
using System.Windows.Input;
using ssk.Models;
using ssk.Services;

namespace ssk.ViewModels;

public class NutritionDetailViewModel : BaseViewModel
{
    private readonly MealPlanRepository _mealPlanRepository;
    private readonly NutritionRepository _nutritionRepository;

    // already intake
    private double _caloriesIntake;
    public double CaloriesIntake { get => _caloriesIntake; set => SetProperty(ref _caloriesIntake, value); }

    private double _proteinIntake;
    public double ProteinIntake { get => _proteinIntake; set => SetProperty(ref _proteinIntake, value); }

    private double _carbsIntake;
    public double CarbsIntake { get => _carbsIntake; set => SetProperty(ref _carbsIntake, value); }

    private double _fatIntake;
    public double FatIntake { get => _fatIntake; set => SetProperty(ref _fatIntake, value); }

    /* target value */
    private double _caloriesTarget;
    public double CaloriesTarget { get => _caloriesTarget; set => SetProperty(ref _caloriesTarget, value); }

    private double _proteinTarget;
    public double ProteinTarget { get => _proteinTarget; set => SetProperty(ref _proteinTarget, value); }

    private double _carbsTarget;
    public double CarbsTarget { get => _carbsTarget; set => SetProperty(ref _carbsTarget, value); }

    private double _fatTarget;
    public double FatTarget { get => _fatTarget; set => SetProperty(ref _fatTarget, value); }

    // progress value 0 to 1
    private double _caloriesProgress;
    public double CaloriesProgress { get => _caloriesProgress; set => SetProperty(ref _caloriesProgress, value); }

    private double _proteinProgress;
    public double ProteinProgress { get => _proteinProgress; set => SetProperty(ref _proteinProgress, value); }

    private double _carbsProgress;
    public double CarbsProgress { get => _carbsProgress; set => SetProperty(ref _carbsProgress, value); }

    private double _fatProgress;
    public double FatProgress { get => _fatProgress; set => SetProperty(ref _fatProgress, value); }

    private double _fiberProgress;
    public double FiberProgress { get => _fiberProgress; set => SetProperty(ref _fiberProgress, value); }

    /* percentage */
    private double _proteinPercent;
    public double ProteinPercent { get => _proteinPercent; set => SetProperty(ref _proteinPercent, value); }

    private double _carbsPercent;
    public double CarbsPercent { get => _carbsPercent; set => SetProperty(ref _carbsPercent, value); }

    private double _fatPercent;
    public double FatPercent { get => _fatPercent; set => SetProperty(ref _fatPercent, value); }

    // calories summary text
    private string _caloriesSummary = string.Empty;
    public string CaloriesSummary { get => _caloriesSummary; set => SetProperty(ref _caloriesSummary, value); }

    // weekly trend
    private double _mondayProgress;
    public double MondayProgress { get => _mondayProgress; set => SetProperty(ref _mondayProgress, value); }

    private double _tuesdayProgress;
    public double TuesdayProgress { get => _tuesdayProgress; set => SetProperty(ref _tuesdayProgress, value); }

    private double _wednesdayProgress;
    public double WednesdayProgress { get => _wednesdayProgress; set => SetProperty(ref _wednesdayProgress, value); }

    private double _thursdayProgress;
    public double ThursdayProgress { get => _thursdayProgress; set => SetProperty(ref _thursdayProgress, value); }

    private double _fridayProgress;
    public double FridayProgress { get => _fridayProgress; set => SetProperty(ref _fridayProgress, value); }

    private double _saturdayProgress;
    public double SaturdayProgress { get => _saturdayProgress; set => SetProperty(ref _saturdayProgress, value); }

    private double _sundayProgress;
    public double SundayProgress { get => _sundayProgress; set => SetProperty(ref _sundayProgress, value); }

    /* diet suggestion list */
    public ObservableCollection<string> Suggestions { get; } = new();

    public ICommand RefreshCommand { get; }

    public NutritionDetailViewModel(
        MealPlanRepository mealPlanRepository,
        NutritionRepository nutritionRepository)
    {
        _mealPlanRepository = mealPlanRepository;
        _nutritionRepository = nutritionRepository;
        Title = "Nutrition Detail";

        RefreshCommand = CreateAsyncCommand(LoadDataAsync);
    }

    public async Task LoadDataAsync()
    {
        try
        {
            var today = DateTime.Today.ToString("yyyy-MM-dd");
            var target = await _nutritionRepository.GetOrCreateForDateAsync(today);
            var summary = await _mealPlanRepository.GetDailySummaryAsync(today);

            summary.CalorieGoal = target.CaloriesGoal;
            summary.ProteinGoal = target.ProteinGoal;
            summary.CarbsGoal = target.CarbsGoal;
            summary.FatGoal = target.FatGoal;
            summary.FiberGoal = target.FiberGoal;

            // already eat only count IsCompleted equal true
            CaloriesIntake = summary.TotalCalories;
            ProteinIntake = summary.TotalProtein;
            CarbsIntake = summary.TotalCarbs;
            FatIntake = summary.TotalFat;

            // target value
            CaloriesTarget = summary.CalorieGoal;
            ProteinTarget = summary.ProteinGoal;
            CarbsTarget = summary.CarbsGoal;
            FatTarget = summary.FatGoal;

            // progress calculate
            CaloriesProgress = CaloriesTarget > 0 ? Math.Min(CaloriesIntake / CaloriesTarget, 1.0) : 0;
            ProteinProgress = ProteinTarget > 0 ? Math.Min(ProteinIntake / ProteinTarget, 1.0) : 0;
            CarbsProgress = CarbsTarget > 0 ? Math.Min(CarbsIntake / CarbsTarget, 1.0) : 0;
            FatProgress = FatTarget > 0 ? Math.Min(FatIntake / FatTarget, 1.0) : 0;
            FiberProgress = summary.FiberGoal > 0 ? Math.Min(summary.TotalFiber / summary.FiberGoal, 1.0) : 0;

            /* three big macro nutrient ratio by calorie protein 4kcal/g carb 4kcal/g fat 9kcal/g */
            var totalMacroCal = ProteinIntake * 4 + CarbsIntake * 4 + FatIntake * 9;
            ProteinPercent = totalMacroCal > 0 ? ProteinIntake * 4 / totalMacroCal : 0;
            CarbsPercent = totalMacroCal > 0 ? CarbsIntake * 4 / totalMacroCal : 0;
            FatPercent = totalMacroCal > 0 ? FatIntake * 9 / totalMacroCal : 0;

            // calories summary text
            CaloriesSummary = $"Already intake {CaloriesIntake:F0} / {CaloriesTarget:F0} kcal";

            // weekly trend load
            await LoadWeeklyTrendAsync();

            /* diet suggestion generate */
            Suggestions.Clear();
            var advice = NutritionCalculator.GenerateAdvice(summary);
            foreach (var item in advice)
                Suggestions.Add(item);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NutritionDetailViewModel] LoadDataAsync error: {ex.Message}");
        }
    }

    private async Task LoadWeeklyTrendAsync()
    {
        try
        {
            var today = DateTime.Today;
            int diff = (int)today.DayOfWeek - (int)DayOfWeek.Monday;
            if (diff < 0) diff += 7;
            var monday = today.AddDays(-diff);

            var dailyGoals = new List<double>();
            for (int i = 0; i < 7; i++)
            {
                var date = monday.AddDays(i).ToString("yyyy-MM-dd");
                var target = await _nutritionRepository.GetOrCreateForDateAsync(date);
                dailyGoals.Add(target.CaloriesGoal);
            }

            // set each day calorie progress
            var progresses = new[] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
            for (int i = 0; i < 7; i++)
            {
                var date = monday.AddDays(i).ToString("yyyy-MM-dd");
                var summary = await _mealPlanRepository.GetDailySummaryAsync(date);
                var goal = dailyGoals[i];
                progresses[i] = goal > 0 ? Math.Min(summary.TotalCalories / goal, 1.0) : 0;
            }

            MondayProgress = progresses[0];
            TuesdayProgress = progresses[1];
            WednesdayProgress = progresses[2];
            ThursdayProgress = progresses[3];
            FridayProgress = progresses[4];
            SaturdayProgress = progresses[5];
            SundayProgress = progresses[6];
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NutritionDetailViewModel] LoadWeeklyTrend error: {ex.Message}");
        }
    }
}
