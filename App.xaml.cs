using ssk.Services;
using ssk.Views;

namespace ssk;

public partial class App : Application
{
    private readonly SeedDataService _seedDataService;
    private readonly ThemeService _themeService;
    private readonly AuthService _authService;
    private readonly AppShell _shell;

    public App(AppShell shell, SeedDataService seedDataService, ThemeService themeService, AuthService authService)
    {
        InitializeComponent();
        _shell = shell;
        _seedDataService = seedDataService;
        _themeService = themeService;
        _authService = authService;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(_shell);

        // when window created wait seed data init finish
        _ = InitializeAsync();

        return window;
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
            System.Diagnostics.Debug.WriteLine($"[App] Init error: {ex.Message}");
        }
    }
}
