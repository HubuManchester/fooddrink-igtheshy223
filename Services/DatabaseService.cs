using SQLite;
using ssk.Models;

namespace ssk.Services;

public class DatabaseService
{
    private const string DbFileName = "ssk.db3";
    private const SQLiteOpenFlags Flags =
        SQLiteOpenFlags.ReadWrite |
        SQLiteOpenFlags.Create |
        SQLiteOpenFlags.SharedCache;

    private readonly SQLiteAsyncConnection _database;
    private bool _initialized = false;
    private readonly object _lock = new();

    public SQLiteAsyncConnection Database => _database;

    public DatabaseService()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, DbFileName);
        _database = new SQLiteAsyncConnection(dbPath, Flags);
    }

    public async Task Init()
    {
        if (_initialized) return;

        lock (_lock)
        {
            if (_initialized) return;
        }

        try
        {
            await _database.CreateTablesAsync(
                CreateFlags.None,
                typeof(Recipe),
                typeof(Ingredient),
                typeof(MealPlan),
                typeof(ShoppingItem),
                typeof(NutritionTarget),
                typeof(UserPreference),
                typeof(FoodRecognitionLog),
                typeof(UserAccount)
            );

            // 迁移：为已有表添加 UserId 列
            await MigrateAddUserIdColumnAsync("ingredients");
            await MigrateAddUserIdColumnAsync("meal_plans");
            await MigrateAddUserIdColumnAsync("shopping_items");
            await MigrateAddUserIdColumnAsync("nutrition_targets");
            await MigrateAddUserIdColumnAsync("food_recognition_logs");

            lock (_lock)
            {
                _initialized = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DatabaseService] Init error: {ex.Message}");
        }
    }

    /// <summary>
    /// 检查表中是否存在 UserId 列，不存在则添加
    /// </summary>
    private async Task MigrateAddUserIdColumnAsync(string tableName)
    {
        try
        {
            var info = await _database.GetTableInfoAsync(tableName);
            if (!info.Any(c => c.Name == "UserId"))
            {
                await _database.ExecuteAsync(
                    $"ALTER TABLE {tableName} ADD COLUMN UserId INTEGER NOT NULL DEFAULT 0");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DatabaseService] Migrate {tableName} error: {ex.Message}");
        }
    }
}
