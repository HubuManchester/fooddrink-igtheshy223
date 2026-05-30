using SQLite;
using ssk.Models;

namespace ssk.Services;

public class RecipeRepository
{
    private readonly DatabaseService _db;

    public RecipeRepository(DatabaseService db)
    {
        _db = db;
    }

    public async Task<List<Recipe>> GetAllAsync()
    {
        try
        {
            await _db.Init();
            return await _db.Database.Table<Recipe>()
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecipeRepository] GetAllAsync error: {ex.Message}");
            return new();
        }
    }

    public async Task<Recipe?> GetByIdAsync(int id)
    {
        try
        {
            await _db.Init();
            return await _db.Database.Table<Recipe>()
                .Where(r => r.Id == id)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecipeRepository] GetByIdAsync error: {ex.Message}");
            return null;
        }
    }

    public async Task<List<Recipe>> SearchAsync(string keyword)
    {
        try
        {
            await _db.Init();
            var all = await _db.Database.Table<Recipe>()
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            return all.Where(r =>
                r.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                (r.Description != null && r.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecipeRepository] SearchAsync error: {ex.Message}");
            return new();
        }
    }

    public async Task<List<Recipe>> GetByCategoryAsync(string category)
    {
        try
        {
            await _db.Init();
            return await _db.Database.Table<Recipe>()
                .Where(r => r.Category == category)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecipeRepository] GetByCategoryAsync error: {ex.Message}");
            return new();
        }
    }

    public async Task<List<Recipe>> GetFavoritesAsync()
    {
        try
        {
            await _db.Init();
            return await _db.Database.Table<Recipe>()
                .Where(r => r.IsFavorite)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecipeRepository] GetFavoritesAsync error: {ex.Message}");
            return new();
        }
    }

    public async Task<List<Recipe>> GetRandomRecommendationsAsync(int count)
    {
        try
        {
            await _db.Init();
            var all = await _db.Database.Table<Recipe>().ToListAsync();
            return all.OrderBy(_ => Guid.NewGuid()).Take(count).ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecipeRepository] GetRandomRecommendationsAsync error: {ex.Message}");
            return new();
        }
    }

    public async Task<int> SaveAsync(Recipe recipe)
    {
        try
        {
            await _db.Init();
            if (recipe.Id > 0)
            {
                recipe.UpdatedAt = DateTime.Now;
                return await _db.Database.UpdateAsync(recipe);
            }
            else
            {
                recipe.CreatedAt = DateTime.Now;
                recipe.UpdatedAt = DateTime.Now;
                return await _db.Database.InsertAsync(recipe);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecipeRepository] SaveAsync error: {ex.Message}");
            return 0;
        }
    }

    public async Task<int> DeleteAsync(Recipe recipe)
    {
        try
        {
            await _db.Init();
            return await _db.Database.DeleteAsync(recipe);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecipeRepository] DeleteAsync error: {ex.Message}");
            return 0;
        }
    }

    public async Task ToggleFavoriteAsync(Recipe recipe)
    {
        try
        {
            recipe.IsFavorite = !recipe.IsFavorite;
            await SaveAsync(recipe);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecipeRepository] ToggleFavoriteAsync error: {ex.Message}");
        }
    }
}
