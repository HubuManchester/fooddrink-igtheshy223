namespace ssk.Services;

public class ThemeService
{
    private readonly UserProfileRepository _userProfileRepo;

    public AppTheme CurrentTheme { get; private set; } = AppTheme.Unspecified;
    public double FontScale { get; private set; } = 1.0;

    public event Action? ThemeChanged;
    public event Action? FontScaleChanged;

    /// <summary>Font size resource name and base value mapping</summary>
    private static readonly Dictionary<string, double> FontResources = new()
    {
        ["Font11"] = 11, ["Font12"] = 12, ["Font13"] = 13,
        ["Font14"] = 14, ["Font15"] = 15, ["Font16"] = 16,
        ["Font18"] = 18, ["Font20"] = 20, ["Font22"] = 22,
        ["Font28"] = 28
    };

    public ThemeService(UserProfileRepository userProfileRepo)
    {
        _userProfileRepo = userProfileRepo;
    }

    public async Task LoadSettingsAsync()
    {
        var theme = await _userProfileRepo.GetThemeAsync();
        ApplyTheme(theme);
        var fontSize = await _userProfileRepo.GetFontSizeAsync();
        ApplyFontSize(fontSize);
    }

    public async Task SetThemeAsync(string theme)
    {
        await _userProfileRepo.SetThemeAsync(theme);
        ApplyTheme(theme);
        ThemeChanged?.Invoke();
    }

    public async Task SetFontSizeAsync(string fontSize)
    {
        await _userProfileRepo.SetFontSizeAsync(fontSize);
        ApplyFontSize(fontSize);
        FontScaleChanged?.Invoke();
    }

    private void ApplyTheme(string theme)
    {
        CurrentTheme = theme switch
        {
            "Light" => AppTheme.Light,
            "Dark" => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };
        Application.Current!.UserAppTheme = CurrentTheme;
    }

    private void ApplyFontSize(string fontSize)
    {
        FontScale = fontSize switch
        {
            "Small" => 0.8,
            "Large" => 1.2,
            "ExtraLarge" => 1.4,
            _ => 1.0
        };

        UpdateFontSizeResources();
    }

    /// <summary>Update global font size resources based on FontScale, all DynamicResource references will auto refresh</summary>
    private void UpdateFontSizeResources()
    {
        if (Application.Current == null) return;

        var res = Application.Current.Resources;
        foreach (var kvp in FontResources)
        {
            res[kvp.Key] = kvp.Value * FontScale;
        }
    }

    public double GetScaledSize(double baseSize) => baseSize * FontScale;
}
