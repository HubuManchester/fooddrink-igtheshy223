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

    // 调试属性
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

    // XAML 绑定的营养属性
    public double TotalCalories => DayNutrition?.TotalCalories ?? 0;
    public double TotalProtein => DayNutrition?.TotalProtein ?? 0;
    public double TotalCarbs => DayNutrition?.TotalCarbs ?? 0;
    public double TotalFat => DayNutrition?.TotalFat ?? 0;
    public double TotalFiber => DayNutrition?.TotalFiber ?? 0;

    // XAML 绑定的各餐段计划
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

    // XAML 绑定的命令（名字必须和 MealPlanPage.xaml 中的绑定一致）
    public ICommand DateSelectedCommand { get; }
    public ICommand ToggleMealCommand { get; }
    public ICommand AddMealPlanCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand LoginCommand { get; }

    private static readonly List<string> MealTypes = new() { "早餐", "午餐", "晚餐", "加餐" };

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
        Title = "饮食计划";

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

            // 按餐段分组
            BreakfastPlans.Clear();
            LunchPlans.Clear();
            DinnerPlans.Clear();
            SnackPlans.Clear();

            foreach (var plan in plans)
            {
                System.Diagnostics.Debug.WriteLine($"[MealPlanVM] Plan: id={plan.Id}, type={plan.MealType}, name={plan.CustomFoodName}, cal={plan.Calories}");
                switch (plan.MealType)
                {
                    case "早餐": BreakfastPlans.Add(plan); break;
                    case "午餐": LunchPlans.Add(plan); break;
                    case "晚餐": DinnerPlans.Add(plan); break;
                    case "加餐": SnackPlans.Add(plan); break;
                }
            }

            System.Diagnostics.Debug.WriteLine($"[MealPlanVM] After split: BF={BreakfastPlans.Count}, Lunch={LunchPlans.Count}, Dinner={DinnerPlans.Count}, Snack={SnackPlans.Count}");

            // 加载营养摘要
            var target = await _nutritionRepository.GetOrCreateForDateAsync(dateStr);
            var summary = await _mealPlanRepository.GetDailySummaryAsync(dateStr);
            summary.CalorieGoal = target.CaloriesGoal;
            summary.ProteinGoal = target.ProteinGoal;
            summary.CarbsGoal = target.CarbsGoal;
            summary.FatGoal = target.FatGoal;
            summary.FiberGoal = target.FiberGoal;
            DayNutrition = summary;

            // 加载本周有计划的日期
            await LoadDatesWithPlansAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MealPlanViewModel] LoadDayPlansAsync error: {ex.Message}");
        }
        finally
        {
            // 延迟解除加载标志，等 UI 绑定完成后再允许用户交互
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
            // 双向绑定已经将 plan.IsCompleted 更新为新值，直接保存
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
                var mealType = await Shell.Current.DisplayActionSheet("选择餐段", "取消", null, MealTypes.ToArray());
                if (string.IsNullOrEmpty(mealType) || mealType == "取消") return;
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
