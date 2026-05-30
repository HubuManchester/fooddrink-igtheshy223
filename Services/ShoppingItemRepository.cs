using SQLite;
using ssk.Models;

namespace ssk.Services;

public class ShoppingItemRepository
{
    private readonly DatabaseService _db;

    public ShoppingItemRepository(DatabaseService db)
    {
        _db = db;
    }

    public async Task<List<ShoppingItem>> GetAllAsync()
    {
        try
        {
            await _db.Init();
            return await _db.Database.Table<ShoppingItem>()
                .OrderBy(i => i.Category)
                .ThenBy(i => i.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ShoppingItemRepository] GetAllAsync error: {ex.Message}");
            return new();
        }
    }

    public async Task<List<ShoppingItem>> GetUnpurchasedAsync()
    {
        try
        {
            await _db.Init();
            return await _db.Database.Table<ShoppingItem>()
                .Where(i => !i.IsPurchased)
                .OrderBy(i => i.Category)
                .ThenBy(i => i.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ShoppingItemRepository] GetUnpurchasedAsync error: {ex.Message}");
            return new();
        }
    }

    public async Task<int> SaveAsync(ShoppingItem item)
    {
        try
        {
            await _db.Init();
            if (item.Id > 0)
            {
                return await _db.Database.UpdateAsync(item);
            }
            else
            {
                item.CreatedAt = DateTime.Now;
                return await _db.Database.InsertAsync(item);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ShoppingItemRepository] SaveAsync error: {ex.Message}");
            return 0;
        }
    }

    public async Task<int> DeleteAsync(ShoppingItem item)
    {
        try
        {
            await _db.Init();
            return await _db.Database.DeleteAsync(item);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ShoppingItemRepository] DeleteAsync error: {ex.Message}");
            return 0;
        }
    }

    public async Task MarkPurchasedAsync(ShoppingItem item, bool purchased)
    {
        try
        {
            item.IsPurchased = purchased;
            await SaveAsync(item);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ShoppingItemRepository] MarkPurchasedAsync error: {ex.Message}");
        }
    }

    public async Task<int> ClearPurchasedAsync()
    {
        try
        {
            await _db.Init();
            var purchased = await _db.Database.Table<ShoppingItem>()
                .Where(i => i.IsPurchased)
                .ToListAsync();
            int count = 0;
            foreach (var item in purchased)
            {
                count += await _db.Database.DeleteAsync(item);
            }
            return count;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ShoppingItemRepository] ClearPurchasedAsync error: {ex.Message}");
            return 0;
        }
    }

    public async Task<int> GetUnpurchasedCountAsync()
    {
        try
        {
            await _db.Init();
            var unpurchased = await _db.Database.Table<ShoppingItem>()
                .Where(i => !i.IsPurchased)
                .ToListAsync();
            return unpurchased.Count;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ShoppingItemRepository] GetUnpurchasedCountAsync error: {ex.Message}");
            return 0;
        }
    }
}
