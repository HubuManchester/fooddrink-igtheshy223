using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using ssk.Models;
using ssk.Services;
using ssk.Views;

namespace ssk.ViewModels;

[QueryProperty(nameof(Action), "action")]
[QueryProperty(nameof(RecipeIdParam), "recipeId")]
[QueryProperty(nameof(MealTypeParam), "mealType")]
public class MealPlanViewModel : BaseViewModel
{
    private readonly MealPlanRepository _mealPlanRepository;
    private readonly NutritionRepository _nutritionRepository;
    private readonly IServiceProvider _serviceProvider;
    private readonly AuthService _authService;

    private string? _action;
    public string? Action
    {
        get => _action;
        set => SetProperty(ref _action, value);
    }

    private string? _recipeIdParam;
    public string? RecipeIdParam
    {
        get => _recipeIdParam;
        set => SetProperty(ref _recipeIdParam, value);
    }

    private string? _mealTypeParam;
    public string? MealTypeParam
    {
        get => _mealTypeParam;
        set => SetProperty(ref _mealTypeParam, value);
    }

    private DateTime _selectedDate = DateTime.Today;
    public DateTime SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (SetProperty(ref _selectedDate, value))
            {
                OnPropertyChanged(nameof(DatesWithPlansCount));
                _ = LoadDayPlansAsync();
            }
        }
    }

    /* debug property */
    public int DatesWithPlansCount => DatesWithPlans?.Count ?? 0;

    private DailyNutritionSummary? _dayNutrition;
    public DailyNutritionSummary? DayNutrition
    {
        get => _dayNutrition;
        set
        {
            if (SetProperty(ref _dayNutrition, value))
            {
                OnPropertyChanged(nameof(TotalCalories));
                OnPropertyChanged(nameof(TotalProtein));
                OnPropertyChanged(nameof(TotalCarbs));
                OnPropertyChanged(nameof(TotalFat));
                OnPropertyChanged(nameof(TotalFiber));
            }
        }
    }

    // XAML bind nutrition properties
    public double TotalCalories => DayNutrition?.TotalCalories ?? 0;
    public double TotalProtein => DayNutrition?.TotalProtein ?? 0;
    public double TotalCarbs => DayNutrition?.TotalCarbs ?? 0;
    public double TotalFat => DayNutrition?.TotalFat ?? 0;
    public double TotalFiber => DayNutrition?.TotalFiber ?? 0;

    /* XAML bind each meal type plan list */
    public ObservableCollection<MealPlan> BreakfastPlans { get; } = new();
    public ObservableCollection<MealPlan> LunchPlans { get; } = new();
    public ObservableCollection<MealPlan> DinnerPlans { get; } = new();
    public ObservableCollection<MealPlan> SnackPlans { get; } = new();

    public ObservableCollection<string> DatesWithPlans { get; } = new();

    private bool _isRefreshing;
    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
    }

    private volatile bool _isLoadingPlans;

    // XAML bind command name must match MealPlanPage.xaml binding
    public ICommand DateSelectedCommand { get; }
    public ICommand ToggleMealCommand { get; }
    public ICommand AddMealPlanCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand LoginCommand { get; }

    private static readonly List<string> MealTypes = new() { "Breakfast", "Lunch", "Dinner", "Snack" };

    public bool IsLoggedIn => _authService.IsLoggedIn;

    public MealPlanViewModel(
        MealPlanRepository mealPlanRepository,
        NutritionRepository nutritionRepository,
        IServiceProvider serviceProvider,
        AuthService authService)
    {
        _mealPlanRepository = mealPlanRepository;
        _nutritionRepository = nutritionRepository;
        _serviceProvider = serviceProvider;
        _authService = authService;
        Title = "Diet Plan";

        _authService.LoginStateChanged += OnLoginStateChanged;

        DateSelectedCommand = new Command<DateTime>(d => SelectedDate = d);
        ToggleMealCommand = new Command<MealPlan>(async p =>
        {
            if (_isLoadingPlans) return;
            await ExecuteToggleCompleted(p);
        });
        AddMealPlanCommand = new Command(ExecuteAddPlan);
        LoginCommand = new Command(() => Shell.Current.GoToAsync(nameof(LoginPage)));
        RefreshCommand = new Command(async () =>
        {
            try { await LoadDayPlansAsync(); }
            finally { IsRefreshing = false; }
        });
    }

    public async Task HandleNavigationParametersAsync()
    {
        try
        {
            if (Action == "add" && int.TryParse(RecipeIdParam, out int recipeId) && !string.IsNullOrEmpty(MealTypeParam))
            {
                var nutrition = new NutritionInfo();
                var recipeRepo = _serviceProvider.GetService<RecipeRepository>();
                string foodName = MealTypeParam;

                if (recipeRepo != null && recipeId > 0)
                {
                    var recipe = await recipeRepo.GetByIdAsync(recipeId);
                    if (recipe != null)
                    {
                        foodName = recipe.Title;
                        nutrition = NutritionCalculator.CalculateForRecipe(recipe);
                    }
                }

                var plan = new MealPlan
                {
                    PlanDate = SelectedDate.ToString("yyyy-MM-dd"),
                    MealType = MealTypeParam,
                    RecipeId = recipeId,
                    CustomFoodName = foodName,
                    Servings = 1,
                    Calories = nutrition.Calories,
                    Protein = nutrition.Protein,
                    Carbs = nutrition.Carbs,
                    Fat = nutrition.Fat,
                    Fiber = nutrition.Fiber
                };

                await _mealPlanRepository.SaveAsync(plan);
                Action = null;
                RecipeIdParam = null;
                MealTypeParam = null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MealPlanViewModel] HandleNavigation error: {ex.Message}");
        }
    }

    public async Task LoadDayPlansAsync()
    {
        _isLoadingPlans = true;
        try
        {
            var dateStr = SelectedDate.ToString("yyyy-MM-dd");
            var plans = await _mealPlanRepository.GetByDateAsync(dateStr);

            System.Diagnostics.Debug.WriteLine($"[MealPlanVM] LoadDayPlans: date={dateStr}, plans.Count={plans.Count}");

            // group by meal type
            BreakfastPlans.Clear();
            LunchPlans.Clear();
            DinnerPlans.Clear();
            SnackPlans.Clear();

            foreach (var plan in plans)
            {
                System.Diagnostics.Debug.WriteLine($"[MealPlanVM] Plan: id={plan.Id}, type={plan.MealType}, name={plan.CustomFoodName}, cal={plan.Calories}");
                switch (plan.MealType)
                {
                    case "Breakfast": BreakfastPlans.Add(plan); break;
                    case "Lunch": LunchPlans.Add(plan); break;
                    case "Dinner": DinnerPlans.Add(plan); break;
                    case "Snack": SnackPlans.Add(plan); break;
                }
            }

            System.Diagnostics.Debug.WriteLine($"[MealPlanVM] After split: BF={BreakfastPlans.Count}, Lunch={LunchPlans.Count}, Dinner={DinnerPlans.Count}, Snack={SnackPlans.Count}");

            /* load nutrition summary */
            var target = await _nutritionRepository.GetOrCreateForDateAsync(dateStr);
            var summary = await _mealPlanRepository.GetDailySummaryAsync(dateStr);
            summary.CalorieGoal = target.CaloriesGoal;
            summary.ProteinGoal = target.ProteinGoal;
            summary.CarbsGoal = target.CarbsGoal;
            summary.FatGoal = target.FatGoal;
            summary.FiberGoal = target.FiberGoal;
            DayNutrition = summary;

            // load this week dates that have plan
            await LoadDatesWithPlansAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MealPlanViewModel] LoadDayPlansAsync error: {ex.Message}");
        }
        finally
        {
            /* delay clear loading flag wait UI bind finish then allow user interaction */
            MainThread.BeginInvokeOnMainThread(() => _isLoadingPlans = false);
        }
    }

    private async Task LoadDatesWithPlansAsync()
    {
        try
        {
            var today = DateTime.Today;
            int diff = (int)today.DayOfWeek - (int)DayOfWeek.Monday;
            if (diff < 0) diff += 7;
            var monday = today.AddDays(-diff);

            var startDate = monday.ToString("yyyy-MM-dd");
            var endDate = monday.AddDays(6).ToString("yyyy-MM-dd");
            var dates = await _mealPlanRepository.GetDatesWithPlansAsync(startDate, endDate);

            DatesWithPlans.Clear();
            foreach (var date in dates)
                DatesWithPlans.Add(date);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MealPlanViewModel] LoadDatesWithPlans error: {ex.Message}");
        }
    }

    private async Task ExecuteToggleCompleted(MealPlan plan)
    {
        try
        {
            // two-way binding already update plan.IsCompleted to new value just save
            await _mealPlanRepository.MarkCompletedAsync(plan, plan.IsCompleted);
            await LoadDayPlansAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MealPlanViewModel] ToggleCompleted error: {ex.Message}");
        }
    }

    private void ExecuteAddPlan()
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var mealType = await Shell.Current.DisplayActionSheet("Select Meal Type", "Cancel", null, MealTypes.ToArray());
                if (string.IsNullOrEmpty(mealType) || mealType == "Cancel") return;
                await Shell.Current.GoToAsync($"RecipeListPage?mealType={mealType}&date={SelectedDate:yyyy-MM-dd}");
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MealPlanViewModel] AddPlan error: {ex.Message}");
        }
    }

    private void OnLoginStateChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            OnPropertyChanged(nameof(IsLoggedIn));
            if (_authService.IsLoggedIn)
                _ = LoadDayPlansAsync();
        });
    }
}
