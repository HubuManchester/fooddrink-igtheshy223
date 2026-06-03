using ssk.Services;

namespace ssk.Views;

public partial class AppShell : Shell
{
    private readonly FirstRunService _firstRunService;
    private bool _firstRunChecked;

    public AppShell(FirstRunService firstRunService)
    {
        InitializeComponent();
        _firstRunService = firstRunService;

        /* register detail route */
        Routing.RegisterRoute(nameof(RecipeDetailPage), typeof(RecipeDetailPage));
        Routing.RegisterRoute(nameof(RecipeEditPage), typeof(RecipeEditPage));
        Routing.RegisterRoute(nameof(CameraPage), typeof(CameraPage));
        Routing.RegisterRoute(nameof(NutritionDetailPage), typeof(NutritionDetailPage));
        Routing.RegisterRoute(nameof(ShoppingListPage), typeof(ShoppingListPage));
        Routing.RegisterRoute(nameof(NearbyStoresPage), typeof(NearbyStoresPage));
        Routing.RegisterRoute(nameof(OnboardingPage), typeof(OnboardingPage));
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
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
