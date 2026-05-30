using SQLite;
using ssk.Models;

namespace ssk.Services;

public class IngredientRepository
{
    private readonly DatabaseService _db;

    public IngredientRepository(DatabaseService db)
    {
        _db = db;
    }

    public async Task<List<Ingredient>> GetAllAsync()
    {
        try
        {
            await _db.Init();
            return await _db.Database.Table<Ingredient>()
                .OrderBy(i => i.Category)
                .ThenBy(i => i.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[IngredientRepository] GetAllAsync error: {ex.Message}");
            return new();
        }
    }

    public async Task<Ingredient?> GetByIdAsync(int id)
    {
        try
        {
            await _db.Init();
            return await _db.Database.Table<Ingredient>()
                .Where(i => i.Id == id)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[IngredientRepository] GetByIdAsync error: {ex.Message}");
            return null;
        }
    }

    public async Task<List<Ingredient>> SearchAsync(string keyword)
    {
        try
        {
            await _db.Init();
            var all = await _db.Database.Table<Ingredient>()
                .OrderBy(i => i.Category)
                .ThenBy(i => i.Name)
                .ToListAsync();
            return all.Where(i => i.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[IngredientRepository] SearchAsync error: {ex.Message}");
            return new();
        }
    }

    public async Task<List<Ingredient>> GetByCategoryAsync(string category)
    {
        try
        {
            await _db.Init();
            return await _db.Database.Table<Ingredient>()
                .Where(i => i.Category == category)
                .OrderBy(i => i.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[IngredientRepository] GetByCategoryAsync error: {ex.Message}");
            return new();
        }
    }

    public async Task<List<Ingredient>> GetExpiringSoonAsync(int days)
    {
        try
        {
            await _db.Init();
            var threshold = DateTime.Today.AddDays(days);
            var all = await _db.Database.Table<Ingredient>()
                .ToListAsync();
            return all.Where(i =>
                i.ExpiryDate.HasValue &&
                i.ExpiryDate.Value >= DateTime.Today &&
                i.ExpiryDate.Value <= threshold)
                .OrderBy(i => i.ExpiryDate)
                .ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[IngredientRepository] GetExpiringSoonAsync error: {ex.Message}");
            return new();
        }
    }

    public async Task<List<Ingredient>> GetExpiredAsync()
    {
        try
        {
            await _db.Init();
            var all = await _db.Database.Table<Ingredient>()
                .ToListAsync();
            return all.Where(i =>
                i.ExpiryDate.HasValue &&
                i.ExpiryDate.Value < DateTime.Today)
                .OrderBy(i => i.ExpiryDate)
                .ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[IngredientRepository] GetExpiredAsync error: {ex.Message}");
            return new();
        }
    }

    public async Task<List<Ingredient>> GetByYoloLabelAsync(string label)
    {
        try
        {
            await _db.Init();
            return await _db.Database.Table<Ingredient>()
                .Where(i => i.YoloLabel == label)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[IngredientRepository] GetByYoloLabelAsync error: {ex.Message}");
            return new();
        }
    }

    public async Task<int> SaveAsync(Ingredient item)
    {
        try
        {
            await _db.Init();
            if (item.Id > 0)
            {
                item.UpdatedAt = DateTime.Now;
                return await _db.Database.UpdateAsync(item);
            }
            else
            {
                item.CreatedAt = DateTime.Now;
                item.UpdatedAt = DateTime.Now;
                return await _db.Database.InsertAsync(item);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[IngredientRepository] SaveAsync error: {ex.Message}");
            return 0;
        }
    }

    public async Task<int> DeleteAsync(Ingredient item)
    {
        try
        {
            await _db.Init();
            return await _db.Database.DeleteAsync(item);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[IngredientRepository] DeleteAsync error: {ex.Message}");
            return 0;
        }
    }
}
