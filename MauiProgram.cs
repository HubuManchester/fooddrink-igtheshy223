using Microsoft.Extensions.Logging;
using ssk.Services;
using ssk.ViewModels;
using ssk.Views;

namespace ssk;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("fa-solid-900.ttf", "FA");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // database service register as singleton
        builder.Services.AddSingleton<DatabaseService>();

        /* repository service register as singleton */
        builder.Services.AddSingleton<RecipeRepository>();
        builder.Services.AddSingleton<IngredientRepository>();
        builder.Services.AddSingleton<MealPlanRepository>();
        builder.Services.AddSingleton<ShoppingItemRepository>();
        builder.Services.AddSingleton<NutritionRepository>();
        builder.Services.AddSingleton<UserProfileRepository>();

        // function service register as singleton
        builder.Services.AddSingleton<SeedDataService>();
        builder.Services.AddSingleton<TextToSpeechService>();
        builder.Services.AddSingleton<CameraService>();
        builder.Services.AddSingleton<HapticService>();
        builder.Services.AddSingleton<SensorService>();
        builder.Services.AddSingleton<ThemeService>();
        builder.Services.AddSingleton<FirstRunService>();
        builder.Services.AddSingleton<YoloInferenceService>();
        builder.Services.AddSingleton<GeolocationService>();
        builder.Services.AddSingleton<CompassService>();
        builder.Services.AddSingleton<SpeechRecognitionService>();
        builder.Services.AddSingleton<AuthService>();

        // here we register ViewModel for use
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<RecipeListViewModel>();
        builder.Services.AddTransient<RecipeDetailViewModel>();
        builder.Services.AddTransient<RecipeEditViewModel>();
        builder.Services.AddTransient<MealPlanViewModel>();
        builder.Services.AddTransient<CameraViewModel>();
        builder.Services.AddTransient<ShoppingListViewModel>();
        builder.Services.AddTransient<NearbyStoresViewModel>();
        builder.Services.AddTransient<NutritionDetailViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<OnboardingViewModel>();
        builder.Services.AddTransient<LoginViewModel>();

        /* register Pages */
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<RecipeListPage>();
        builder.Services.AddTransient<RecipeDetailPage>();
        builder.Services.AddTransient<RecipeEditPage>();
        builder.Services.AddTransient<MealPlanPage>();
        builder.Services.AddTransient<CameraPage>();
        builder.Services.AddTransient<ShoppingListPage>();
        builder.Services.AddTransient<NearbyStoresPage>();
        builder.Services.AddTransient<NutritionDetailPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<OnboardingPage>();
        builder.Services.AddTransient<LoginPage>();

        // Shell register as singleton
        builder.Services.AddSingleton<AppShell>();

        return builder.Build();
    }
}
