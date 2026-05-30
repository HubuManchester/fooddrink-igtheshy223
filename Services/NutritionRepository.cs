using SQLite;
using ssk.Models;

namespace ssk.Services;

public class NutritionRepository
{
    private readonly DatabaseService _db;

    public NutritionRepository(DatabaseService db)
    {
        _db = db;
    }

    public async Task<NutritionTarget?> GetByDateAsync(string date)
    {
        try
        {
            await _db.Init();
            return await _db.Database.Table<NutritionTarget>()
                .Where(t => t.TargetDate == date)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NutritionRepository] GetByDateAsync error: {ex.Message}");
            return null;
        }
    }

    public async Task<NutritionTarget> GetOrCreateForDateAsync(string date)
    {
        try
        {
            await _db.Init();

            var existing = await GetByDateAsync(date);
            if (existing != null)
                return existing;

            // 查询最近一天的目标值作为默认值
            var allTargets = await _db.Database.Table<NutritionTarget>()
                .OrderByDescending(t => t.TargetDate)
                .ToListAsync();

            var latest = allTargets.FirstOrDefault(t => string.Compare(t.TargetDate, date) < 0);

            var target = new NutritionTarget
            {
                TargetDate = date,
                CaloriesGoal = latest?.CaloriesGoal ?? 2000,
                ProteinGoal = latest?.ProteinGoal ?? 60,
                CarbsGoal = latest?.CarbsGoal ?? 250,
                FatGoal = latest?.FatGoal ?? 65,
                FiberGoal = latest?.FiberGoal ?? 25
            };

            await _db.Database.InsertAsync(target);
            return target;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NutritionRepository] GetOrCreateForDateAsync error: {ex.Message}");
            return new NutritionTarget { TargetDate = date };
        }
    }

    public async Task<int> SaveAsync(NutritionTarget target)
    {
        try
        {
            await _db.Init();

            var existing = await GetByDateAsync(target.TargetDate);
            if (existing != null)
            {
                target.Id = existing.Id;
                return await _db.Database.UpdateAsync(target);
            }
            else
            {
                return await _db.Database.InsertAsync(target);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NutritionRepository] SaveAsync error: {ex.Message}");
            return 0;
        }
    }
}
