using ssk.Services;

namespace ssk.Views;

public partial class AppShell : Shell
{
    private readonly SeedDataService _seedDataService;
    private readonly FirstRunService _firstRunService;
    private readonly ThemeService _themeService;
    private readonly AuthService _authService;
    private bool _firstRunChecked;

    public AppShell(SeedDataService seedDataService, FirstRunService firstRunService, ThemeService themeService, AuthService authService)
    {
        InitializeComponent();
        _seedDataService = seedDataService;
        _firstRunService = firstRunService;
        _themeService = themeService;
        _authService = authService;

        // 注册层级路由
        Routing.RegisterRoute(nameof(RecipeDetailPage), typeof(RecipeDetailPage));
        Routing.RegisterRoute(nameof(RecipeEditPage), typeof(RecipeEditPage));
        // IngredientEditPage removed
        Routing.RegisterRoute(nameof(CameraPage), typeof(CameraPage));
        Routing.RegisterRoute(nameof(NutritionDetailPage), typeof(NutritionDetailPage));
        Routing.RegisterRoute(nameof(ShoppingListPage), typeof(ShoppingListPage));
        Routing.RegisterRoute(nameof(NearbyStoresPage), typeof(NearbyStoresPage));
        Routing.RegisterRoute(nameof(OnboardingPage), typeof(OnboardingPage));
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
    }

    private async Task InitializeAsync()
    {
        try
        {
            await _seedDataService.InitializeAsync();
            await _themeService.LoadSettingsAsync();
            await _authService.CheckAutoLoginAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Init error: {ex.Message}");
        }
    }

    protected override async void OnNavigated(ShellNavigatedEventArgs args)
    {
        base.OnNavigated(args);

        if (_firstRunChecked) return;
        _firstRunChecked = true;

        try
        {
            var isFirstRun = await _firstRunService.IsFirstRunAsync();
            if (isFirstRun)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await GoToAsync($"//{nameof(OnboardingPage)}");
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"First run check error: {ex.Message}");
        }
    }
}
