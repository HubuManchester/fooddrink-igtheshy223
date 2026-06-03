using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using ssk.Services;
using ssk.Views;

namespace ssk.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private readonly UserProfileRepository _userProfileRepository;
    private readonly ThemeService _themeService;
    private readonly HapticService _hapticService;
    private readonly TextToSpeechService _ttsService;
    private readonly DatabaseService _databaseService;
    private readonly IServiceProvider _serviceProvider;
    private readonly AuthService _authService;

    private string _userName = string.Empty;
    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }

    private bool _isEditingUserName;
    public bool IsEditingUserName
    {
        get => _isEditingUserName;
        set => SetProperty(ref _isEditingUserName, value);
    }

    private string _selectedTheme = "System";
    public string SelectedTheme
    {
        get => _selectedTheme;
        set => SetProperty(ref _selectedTheme, value);
    }

    private string _selectedFontSize = "Medium";
    public string SelectedFontSize
    {
        get => _selectedFontSize;
        set => SetProperty(ref _selectedFontSize, value);
    }

    private double _caloriesGoal = 2000;
    public double CaloriesGoal
    {
        get => _caloriesGoal;
        set => SetProperty(ref _caloriesGoal, value);
    }

    public List<string> Themes { get; } = new() { "Light", "Dark", "System" };
    public List<string> FontSizes { get; } = new() { "Small", "Medium", "Large", "ExtraLarge" };

    private bool _isLoggedIn;
    public bool IsLoggedIn
    {
        get => _isLoggedIn;
        set
        {
            if (SetProperty(ref _isLoggedIn, value))
            {
                OnPropertyChanged(nameof(LoginButtonText));
                OnPropertyChanged(nameof(DisplayUserName));
                OnPropertyChanged(nameof(ShowLoginButton));
                OnPropertyChanged(nameof(ShowLogoutButton));
                OnPropertyChanged(nameof(ShowEditUserName));
            }
        }
    }

    private string _displayUserName = string.Empty;
    public string DisplayUserName
    {
        get => _displayUserName;
        set => SetProperty(ref _displayUserName, value);
    }

    public string LoginButtonText => IsLoggedIn ? "Logout" : "Login / Register";
    public bool ShowLoginButton => !IsLoggedIn;
    public bool ShowLogoutButton => IsLoggedIn;
    public bool ShowEditUserName => IsLoggedIn;

    public ICommand SaveUserNameCommand { get; }
    public ICommand ToggleEditUserNameCommand { get; }
    public ICommand SetThemeCommand { get; }
    public ICommand SetFontSizeCommand { get; }
    public ICommand SaveCaloriesGoalCommand { get; }
    public ICommand TestSpeechCommand { get; }
    public ICommand TestHapticCommand { get; }
    public ICommand NavigateToStoresCommand { get; }
    public ICommand AboutCommand { get; }
    public ICommand ResetDataCommand { get; }
    public ICommand LoginLogoutCommand { get; }

    public SettingsViewModel(
        UserProfileRepository userProfileRepository,
        ThemeService themeService,
        HapticService hapticService,
        TextToSpeechService ttsService,
        DatabaseService databaseService,
        IServiceProvider serviceProvider,
        AuthService authService)
    {
        _userProfileRepository = userProfileRepository;
        _themeService = themeService;
        _hapticService = hapticService;
        _ttsService = ttsService;
        _databaseService = databaseService;
        _serviceProvider = serviceProvider;
        _authService = authService;
        Title = "Settings";

        SaveUserNameCommand = CreateAsyncCommand(ExecuteSaveUserName);
        ToggleEditUserNameCommand = CreateCommand(() => IsEditingUserName = !IsEditingUserName);
        SetThemeCommand = CreateAsyncCommand<string>(ExecuteSetTheme);
        SetFontSizeCommand = CreateAsyncCommand<string>(ExecuteSetFontSize);
        SaveCaloriesGoalCommand = CreateAsyncCommand(ExecuteSaveCaloriesGoal);
        TestSpeechCommand = CreateAsyncCommand(ExecuteTestSpeech);
        TestHapticCommand = CreateAsyncCommand(ExecuteTestHaptic);
        NavigateToStoresCommand = CreateCommand(() =>
            Shell.Current.GoToAsync(nameof(NearbyStoresPage)));
        AboutCommand = CreateCommand(ExecuteAbout);
        ResetDataCommand = CreateAsyncCommand(ExecuteResetData);
        LoginLogoutCommand = CreateAsyncCommand(ExecuteLoginLogout);

        _authService.LoginStateChanged += OnLoginStateChanged;
    }

    public async Task LoadSettingsAsync()
    {
        try
        {
            UserName = await _userProfileRepository.GetUserNameAsync();
            SelectedTheme = await _userProfileRepository.GetThemeAsync();
            SelectedFontSize = await _userProfileRepository.GetFontSizeAsync();

            // login state
            IsLoggedIn = _authService.IsLoggedIn;
            DisplayUserName = _authService.CurrentUser?.Username ?? "Guest Mode";

            /* get current calorie goal */
            var today = DateTime.Today.ToString("yyyy-MM-dd");
            var nutritionRepo = _serviceProvider.GetService<NutritionRepository>();
            if (nutritionRepo != null)
            {
                var target = await nutritionRepo.GetOrCreateForDateAsync(today);
                CaloriesGoal = target.CaloriesGoal;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsViewModel] LoadSettingsAsync error: {ex.Message}");
        }
    }

    private async Task ExecuteSaveUserName()
    {
        try
        {
            await _userProfileRepository.SetUserNameAsync(UserName);
            IsEditingUserName = false;
            await Shell.Current.DisplayAlert("Success", "Username save success", "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsViewModel] SaveUserName error: {ex.Message}");
        }
    }

    private async Task ExecuteSetTheme(string theme)
    {
        try
        {
            SelectedTheme = theme;
            await _themeService.SetThemeAsync(theme);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsViewModel] SetTheme error: {ex.Message}");
        }
    }

    private async Task ExecuteSetFontSize(string fontSize)
    {
        try
        {
            SelectedFontSize = fontSize;
            await _themeService.SetFontSizeAsync(fontSize);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsViewModel] SetFontSize error: {ex.Message}");
        }
    }

    private async Task ExecuteSaveCaloriesGoal()
    {
        try
        {
            var today = DateTime.Today.ToString("yyyy-MM-dd");
            var nutritionRepo = _serviceProvider.GetService<NutritionRepository>();
            if (nutritionRepo != null)
            {
                var target = await nutritionRepo.GetOrCreateForDateAsync(today);
                target.CaloriesGoal = CaloriesGoal;
                await nutritionRepo.SaveAsync(target);
            }
            await Shell.Current.DisplayAlert("Success", "Calorie goal save success", "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsViewModel] SaveCaloriesGoal error: {ex.Message}");
        }
    }

    private async Task ExecuteTestSpeech()
    {
        try
        {
            await _ttsService.SpeakAsync("Speech test success, welcome use Food Assistant");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsViewModel] TestSpeech error: {ex.Message}");
            await Shell.Current.DisplayAlert("Hint", "Speech function not available", "OK");
        }
    }

    private async Task ExecuteTestHaptic()
    {
        try
        {
            await _hapticService.SuccessAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsViewModel] TestHaptic error: {ex.Message}");
        }
    }

    private void ExecuteAbout()
    {
        try
        {
            Shell.Current.DisplayAlert("About",
                "Food Assistant v1.0\n\n" +
                "A smart diet management application\n\n" +
                "Features:\n" +
                "- Recipe management and recommend\n" +
                "- Diet plan making\n" +
                "- Nutrition intake tracking\n" +
                "- Photo recognize food\n" +
                "- Shopping list management\n" +
                "- Nearby store finding",
                "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsViewModel] About error: {ex.Message}");
        }
    }

    private async Task ExecuteResetData()
    {
        try
        {
            var confirm = await Shell.Current.DisplayAlert("Warning",
                "Are you sure clear all data? This operation cannot undo!",
                "Clear Data", "Cancel");
            if (!confirm) return;

            var secondConfirm = await Shell.Current.DisplayAlert("Second Confirm",
                "Really delete all data? Include all recipe, plan, ingredient etc.",
                "Confirm Clear", "Let me think");
            if (!secondConfirm) return;

            await _databaseService.Init();
            await _databaseService.Database.DropTableAsync<ssk.Models.Recipe>();
            await _databaseService.Database.DropTableAsync<ssk.Models.Ingredient>();
            await _databaseService.Database.DropTableAsync<ssk.Models.MealPlan>();
            await _databaseService.Database.DropTableAsync<ssk.Models.ShoppingItem>();
            await _databaseService.Database.DropTableAsync<ssk.Models.NutritionTarget>();
            await _databaseService.Database.DropTableAsync<ssk.Models.UserPreference>();
            await _databaseService.Database.DropTableAsync<ssk.Models.FoodRecognitionLog>();
            await _databaseService.Init(); // rebuild table

            await Shell.Current.DisplayAlert("Done", "All data already clear", "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsViewModel] ResetData error: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", "Clear data fail", "OK");
        }
    }

    private async Task ExecuteLoginLogout()
    {
        try
        {
            if (_authService.IsLoggedIn)
            {
                var confirm = await Shell.Current.DisplayAlert("Hint",
                    "Are you sure logout?", "Logout", "Cancel");
                if (!confirm) return;

                await _authService.LogoutAsync();
            }
            else
            {
                await Shell.Current.GoToAsync(nameof(LoginPage));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsViewModel] LoginLogout error: {ex.Message}");
        }
    }

    private void OnLoginStateChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            IsLoggedIn = _authService.IsLoggedIn;
            DisplayUserName = _authService.CurrentUser?.Username ?? "Guest Mode";
            if (IsLoggedIn)
            {
                UserName = DisplayUserName;
            }
        });
    }
}
