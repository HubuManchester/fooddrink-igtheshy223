using System.Windows.Input;
using ssk.Services;

namespace ssk.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly AuthService _authService;

    private string _username = string.Empty;
    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    private string _confirmPassword = string.Empty;
    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetProperty(ref _confirmPassword, value);
    }

    private bool _isRegisterMode;
    public bool IsRegisterMode
    {
        get => _isRegisterMode;
        set
        {
            if (SetProperty(ref _isRegisterMode, value))
            {
                OnPropertyChanged(nameof(ModeTitle));
                OnPropertyChanged(nameof(ToggleText));
                OnPropertyChanged(nameof(ShowConfirmPassword));
            }
        }
    }

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public string ModeTitle => IsRegisterMode ? "注册" : "登录";
    public string ToggleText => IsRegisterMode ? "已有账号？去登录" : "没有账号？去注册";
    public bool ShowConfirmPassword => IsRegisterMode;

    public ICommand LoginCommand { get; }
    public ICommand RegisterCommand { get; }
    public ICommand ToggleModeCommand { get; }

    public LoginViewModel(AuthService authService)
    {
        _authService = authService;
        Title = "登录";

        LoginCommand = CreateAsyncCommand(ExecuteLogin);
        RegisterCommand = CreateAsyncCommand(ExecuteRegister);
        ToggleModeCommand = CreateCommand(() => IsRegisterMode = !IsRegisterMode);
    }

    private async Task ExecuteLogin()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "请输入用户名和密码";
            return;
        }

        var result = await _authService.LoginAsync(Username, Password);
        if (result.Success)
        {
            await Shell.Current.GoToAsync("..");
        }
        else
        {
            ErrorMessage = result.Message;
        }
    }

    private async Task ExecuteRegister()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "请输入用户名和密码";
            return;
        }

        if (Username.Length < 2)
        {
            ErrorMessage = "用户名至少2个字符";
            return;
        }

        if (Password.Length < 4)
        {
            ErrorMessage = "密码至少4个字符";
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "两次密码输入不一致";
            return;
        }

        var result = await _authService.RegisterAsync(Username, Password);
        if (result.Success)
        {
            await Shell.Current.GoToAsync("..");
        }
        else
        {
            ErrorMessage = result.Message;
        }
    }
}
