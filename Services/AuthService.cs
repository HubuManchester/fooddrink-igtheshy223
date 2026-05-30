using System.Security.Cryptography;
using System.Text;
using ssk.Models;

namespace ssk.Services;

public class AuthService
{
    private readonly DatabaseService _db;
    private readonly UserProfileRepository _userProfileRepository;

    private UserAccount? _currentUser;

    /// <summary>当前登录用户，null 表示游客模式</summary>
    public UserAccount? CurrentUser
    {
        get => _currentUser;
        private set
        {
            _currentUser = value;
            LoginStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>是否已登录</summary>
    public bool IsLoggedIn => CurrentUser != null;

    /// <summary>当前用户ID，未登录返回 0（游客）</summary>
    public int CurrentUserId => CurrentUser?.Id ?? 0;

    /// <summary>登录状态变化事件</summary>
    public event EventHandler? LoginStateChanged;

    public AuthService(DatabaseService db, UserProfileRepository userProfileRepository)
    {
        _db = db;
        _userProfileRepository = userProfileRepository;
    }

    /// <summary>启动时从 UserPreference 恢复登录状态</summary>
    public async Task CheckAutoLoginAsync()
    {
        try
        {
            await _db.Init();
            var userIdStr = await _userProfileRepository.GetAsync("current_user_id");
            if (int.TryParse(userIdStr, out int userId))
            {
                var user = await _db.Database.Table<UserAccount>()
                    .Where(u => u.Id == userId)
                    .FirstOrDefaultAsync();
                if (user != null)
                {
                    user.LastLoginAt = DateTime.UtcNow;
                    await _db.Database.UpdateAsync(user);
                    CurrentUser = user;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthService] CheckAutoLogin error: {ex.Message}");
        }
    }

    /// <summary>注册新用户</summary>
    public async Task<AuthResult> RegisterAsync(string username, string password)
    {
        try
        {
            await _db.Init();

            // 检查用户名是否已存在
            var existing = await _db.Database.Table<UserAccount>()
                .Where(u => u.Username == username)
                .FirstOrDefaultAsync();
            if (existing != null)
                return AuthResult.Fail("用户名已存在");

            var user = new UserAccount
            {
                Username = username,
                PasswordHash = HashPassword(password),
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            await _db.Database.InsertAsync(user);
            await SaveCurrentUserIdAsync(user.Id);
            CurrentUser = user;
            return AuthResult.Ok(user);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthService] Register error: {ex.Message}");
            return AuthResult.Fail("注册失败，请重试");
        }
    }

    /// <summary>用户登录</summary>
    public async Task<AuthResult> LoginAsync(string username, string password)
    {
        try
        {
            await _db.Init();

            var user = await _db.Database.Table<UserAccount>()
                .Where(u => u.Username == username)
                .FirstOrDefaultAsync();

            if (user == null)
                return AuthResult.Fail("用户不存在");

            if (user.PasswordHash != HashPassword(password))
                return AuthResult.Fail("密码错误");

            user.LastLoginAt = DateTime.UtcNow;
            await _db.Database.UpdateAsync(user);
            await SaveCurrentUserIdAsync(user.Id);
            CurrentUser = user;
            return AuthResult.Ok(user);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthService] Login error: {ex.Message}");
            return AuthResult.Fail("登录失败，请重试");
        }
    }

    /// <summary>退出登录</summary>
    public async Task LogoutAsync()
    {
        try
        {
            await _userProfileRepository.SetAsync("current_user_id", string.Empty);
            CurrentUser = null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthService] Logout error: {ex.Message}");
        }
    }

    /// <summary>获取登录提示是否已被关闭</summary>
    public async Task<bool> IsLoginPromptDismissedAsync()
    {
        var value = await _userProfileRepository.GetAsync("login_prompt_dismissed");
        return value == "true";
    }

    /// <summary>关闭登录提示</summary>
    public async Task DismissLoginPromptAsync()
    {
        await _userProfileRepository.SetAsync("login_prompt_dismissed", "true");
    }

    private async Task SaveCurrentUserIdAsync(int userId)
    {
        await _userProfileRepository.SetAsync("current_user_id", userId.ToString());
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }
}

/// <summary>认证操作结果</summary>
public class AuthResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public UserAccount? User { get; init; }

    public static AuthResult Ok(UserAccount user) => new() { Success = true, User = user };
    public static AuthResult Fail(string message) => new() { Success = false, Message = message };
}
