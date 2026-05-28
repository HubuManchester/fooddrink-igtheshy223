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

        // 在窗口创建后立即等待种子数据初始化完成
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
