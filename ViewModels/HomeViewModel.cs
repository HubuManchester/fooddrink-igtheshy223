using System.Collections.ObjectModel;
using System.Windows.Input;
using ssk.Models;
using ssk.Services;
using ssk.Views;

namespace ssk.ViewModels;

public class HomeViewModel : BaseViewModel
{
    private readonly RecipeRepository _recipeRepository;
    private readonly MealPlanRepository _mealPlanRepository;
    private readonly NutritionRepository _nutritionRepository;
    private readonly SensorService _sensorService;
    private readonly HapticService _hapticService;
    private readonly AuthService _authService;

    private string _greeting = string.Empty;
    public string Greeting
    {
        get => _greeting;
        set => SetProperty(ref _greeting, value);
    }

    private string _dateDisplay = string.Empty;
    public string DateDisplay
    {
        get => _dateDisplay;
        set => SetProperty(ref _dateDisplay, value);
    }

    private DailyNutritionSummary? _todayNutrition;
    public DailyNutritionSummary? TodayNutrition
    {
        get => _todayNutrition;
        set
        {
            if (SetProperty(ref _todayNutrition, value))
            {
                OnPropertyChanged(nameof(CaloriesDisplay));
                OnPropertyChanged(nameof(CaloriesProgress));
            }
        }
    }

    public ObservableCollection<Recipe> RecommendedRecipes { get; } = new();

    public string CaloriesDisplay => TodayNutrition != null
        ? $"{TodayNutrition.TotalCalories:F0} / {TodayNutrition.CalorieGoal:F0} kcal" : "0 / 0 kcal";

    public double CaloriesProgress => TodayNutrition?.CalorieGoal > 0
        ? Math.Min(TodayNutrition.TotalCalories / TodayNutrition.CalorieGoal, 1.0) : 0;

    public ICommand NavigateToCameraCommand { get; }
    public ICommand NavigateToShoppingCommand { get; }
    public ICommand ShakeRecommendCommand { get; }
    public ICommand NavigateToStoresCommand { get; }
    public ICommand NavigateToNutritionCommand { get; }
    public ICommand NavigateToRecipeDetailCommand { get; }

    private bool _isRefreshing;
    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
    }

    private bool _showLoginPrompt;
    public bool ShowLoginPrompt
    {
        get => _showLoginPrompt;
        set => SetProperty(ref _showLoginPrompt, value);
    }

    public bool IsLoggedIn => _authService.IsLoggedIn;

    public ICommand RefreshCommand { get; }
    public ICommand LoginCommand { get; }
    public ICommand DismissLoginPromptCommand { get; }

    public HomeViewModel(
        RecipeRepository recipeRepository,
        MealPlanRepository mealPlanRepository,
        NutritionRepository nutritionRepository,
        SensorService sensorService,
        HapticService hapticService,
        AuthService authService)
    {
        _recipeRepository = recipeRepository;
        _mealPlanRepository = mealPlanRepository;
        _nutritionRepository = nutritionRepository;
        _sensorService = sensorService;
        _hapticService = hapticService;
        _authService = authService;

        Title = "首页";

        NavigateToCameraCommand = CreateCommand(() =>
            Shell.Current.GoToAsync(nameof(CameraPage)));

        NavigateToShoppingCommand = CreateCommand(() =>
            Shell.Current.GoToAsync(nameof(ShoppingListPage)));

        ShakeRecommendCommand = CreateCommand(ExecuteShakeRecommend);

        NavigateToStoresCommand = CreateCommand(() =>
            Shell.Current.GoToAsync(nameof(NearbyStoresPage)));

        NavigateToNutritionCommand = CreateCommand(() =>
            Shell.Current.GoToAsync(nameof(NutritionDetailPage)));

        NavigateToRecipeDetailCommand = CreateAsyncCommand<Recipe>(recipe =>
            Shell.Current.GoToAsync($"RecipeDetailPage?RecipeId={recipe.Id}"));

        LoginCommand = CreateCommand(() =>
            Shell.Current.GoToAsync(nameof(LoginPage)));

        DismissLoginPromptCommand = CreateAsyncCommand(ExecuteDismissLoginPrompt);

        RefreshCommand = new Command(async () =>
        {
            try
            {
                await LoadDataAsync();
            }
            finally
            {
                IsRefreshing = false;
            }
        });

        _sensorService.ShakeDetected += OnShakeDetected;
        _authService.LoginStateChanged += OnLoginStateChanged;
    }

    public void Initialize()
    {
        UpdateGreeting();
        UpdateDateDisplay();
        _sensorService.StartMonitoring();
        UpdateLoginPromptState();
    }

    public void Cleanup()
    {
        _sensorService.StopMonitoring();
    }

    private void UpdateGreeting()
    {
        var hour = DateTime.Now.Hour;
        Greeting = hour switch
        {
            < 6 => "夜深了，注意休息",
            < 9 => "早上好，来份营养早餐吧",
            < 12 => "上午好，准备午餐了吗",
            < 14 => "中午好，享用美食吧",
            < 18 => "下午好，来点下午茶",
            < 22 => "晚上好，准备晚餐了吗",
            _ => "夜深了，注意休息"
        };
    }

    private void UpdateDateDisplay()
    {
        var now = DateTime.Now;
        DateDisplay = $"{now.Month}月{now.Day}日";
    }

    public async Task LoadDataAsync()
    {
        try
        {
            var recipes = await _recipeRepository.GetRandomRecommendationsAsync(3);
            RecommendedRecipes.Clear();
            foreach (var recipe in recipes)
                RecommendedRecipes.Add(recipe);

            var today = DateTime.Today.ToString("yyyy-MM-dd");
            var target = await _nutritionRepository.GetOrCreateForDateAsync(today);
            var summary = await _mealPlanRepository.GetDailySummaryAsync(today);
            summary.CalorieGoal = target.CaloriesGoal;
            summary.ProteinGoal = target.ProteinGoal;
            summary.CarbsGoal = target.CarbsGoal;
            summary.FatGoal = target.FatGoal;
            summary.FiberGoal = target.FiberGoal;
            TodayNutrition = summary;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HomeViewModel] LoadDataAsync error: {ex.Message}");
        }
    }

    private async void ExecuteShakeRecommend()
    {
        try
        {
            await _hapticService.LightAsync();
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HomeViewModel] ShakeRecommend error: {ex.Message}");
        }
    }

    private void OnShakeDetected()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                await _hapticService.LightAsync();
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HomeViewModel] OnShakeDetected error: {ex.Message}");
            }
        });
    }

    private async void UpdateLoginPromptState()
    {
        try
        {
            if (_authService.IsLoggedIn)
            {
                ShowLoginPrompt = false;
            }
            else
            {
                var dismissed = await _authService.IsLoginPromptDismissedAsync();
                ShowLoginPrompt = !dismissed;
            }
            OnPropertyChanged(nameof(IsLoggedIn));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HomeViewModel] UpdateLoginPromptState error: {ex.Message}");
        }
    }

    private async Task ExecuteDismissLoginPrompt()
    {
        try
        {
            await _authService.DismissLoginPromptAsync();
            ShowLoginPrompt = false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HomeViewModel] DismissLoginPrompt error: {ex.Message}");
        }
    }

    private void OnLoginStateChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            UpdateLoginPromptState();
        });
    }
}
