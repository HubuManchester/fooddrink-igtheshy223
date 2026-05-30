using SQLite;
using ssk.Models;

namespace ssk.Services;

public class UserProfileRepository
{
    private readonly DatabaseService _db;

    public UserProfileRepository(DatabaseService db)
    {
        _db = db;
    }

    public async Task<string?> GetAsync(string key)
    {
        try
        {
            await _db.Init();
            var pref = await _db.Database.Table<UserPreference>()
                .Where(p => p.Key == key)
                .FirstOrDefaultAsync();
            return pref?.Value;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[UserProfileRepository] GetAsync error: {ex.Message}");
            return null;
        }
    }

    public async Task SetAsync(string key, string value)
    {
        try
        {
            await _db.Init();
            var existing = await _db.Database.Table<UserPreference>()
                .Where(p => p.Key == key)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                existing.Value = value;
                await _db.Database.UpdateAsync(existing);
            }
            else
            {
                await _db.Database.InsertAsync(new UserPreference(key, value));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[UserProfileRepository] SetAsync error: {ex.Message}");
        }
    }

    public async Task<string> GetThemeAsync()
    {
        return await GetAsync("theme") ?? "System";
    }

    public async Task SetThemeAsync(string theme)
    {
        await SetAsync("theme", theme);
    }

    public async Task<string> GetFontSizeAsync()
    {
        return await GetAsync("font_size") ?? "Medium";
    }

    public async Task SetFontSizeAsync(string fontSize)
    {
        await SetAsync("font_size", fontSize);
    }

    public async Task<string> GetUserNameAsync()
    {
        return await GetAsync("user_name") ?? "美食家";
    }

    public async Task SetUserNameAsync(string name)
    {
        await SetAsync("user_name", name);
    }

    public async Task<bool> IsFirstRunAsync()
    {
        var value = await GetAsync("first_run");
        return value != "completed";
    }

    public async Task MarkFirstRunCompletedAsync()
    {
        await SetAsync("first_run", "completed");
    }

    public async Task<int> GetSeedVersionAsync()
    {
        var value = await GetAsync("seed_version");
        return int.TryParse(value, out int version) ? version : 0;
    }

    public async Task SetSeedVersionAsync(int version)
    {
        await SetAsync("seed_version", version.ToString());
    }
}
