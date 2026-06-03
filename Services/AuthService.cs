using System.Security.Cryptography;
using System.Text;
using ssk.Models;

namespace ssk.Services;

public class AuthService
{
    private readonly DatabaseService _db;
    private readonly UserProfileRepository _userProfileRepository;

    private UserAccount? _currentUser;

    /// <summary>Current logged in user, null means guest mode</summary>
    public UserAccount? CurrentUser
    {
        get => _currentUser;
        private set
        {
            _currentUser = value;
            LoginStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>Is user logged in</summary>
    public bool IsLoggedIn => CurrentUser != null;

    /// <summary>Current user ID, returns 0 (guest) if not logged in</summary>
    public int CurrentUserId => CurrentUser?.Id ?? 0;

    /// <summary>Login state changed event</summary>
    public event EventHandler? LoginStateChanged;

    public AuthService(DatabaseService db, UserProfileRepository userProfileRepository)
    {
        _db = db;
        _userProfileRepository = userProfileRepository;
    }

    /// <summary>Restore login state from UserPreference on startup</summary>
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

    /// <summary>Register new user</summary>
    public async Task<AuthResult> RegisterAsync(string username, string password)
    {
        try
        {
            await _db.Init();

            // Check if username already exists
            var existing = await _db.Database.Table<UserAccount>()
                .Where(u => u.Username == username)
                .FirstOrDefaultAsync();
            if (existing != null)
                return AuthResult.Fail("Username already exist");

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
            return AuthResult.Fail("Register fail, please try again");
        }
    }

    /// <summary>User login</summary>
    public async Task<AuthResult> LoginAsync(string username, string password)
    {
        try
        {
            await _db.Init();

            var user = await _db.Database.Table<UserAccount>()
                .Where(u => u.Username == username)
                .FirstOrDefaultAsync();

            if (user == null)
                return AuthResult.Fail("User not exist");

            if (user.PasswordHash != HashPassword(password))
                return AuthResult.Fail("Password wrong");

            user.LastLoginAt = DateTime.UtcNow;
            await _db.Database.UpdateAsync(user);
            await SaveCurrentUserIdAsync(user.Id);
            CurrentUser = user;
            return AuthResult.Ok(user);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthService] Login error: {ex.Message}");
            return AuthResult.Fail("Login fail, please try again");
        }
    }

    /// <summary>Logout</summary>
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

    /// <summary>Check if login prompt has been dismissed</summary>
    public async Task<bool> IsLoginPromptDismissedAsync()
    {
        var value = await _userProfileRepository.GetAsync("login_prompt_dismissed");
        return value == "true";
    }

    /// <summary>Dismiss login prompt</summary>
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

/// <summary>Auth operation result</summary>
public class AuthResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public UserAccount? User { get; init; }

    public static AuthResult Ok(UserAccount user) => new() { Success = true, User = user };
    public static AuthResult Fail(string message) => new() { Success = false, Message = message };
}
