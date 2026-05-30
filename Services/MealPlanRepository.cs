using SQLite;
using ssk.Models;

namespace ssk.Services;

public class MealPlanRepository
{
    private readonly DatabaseService _db;

    public MealPlanRepository(DatabaseService db)
    {
        _db = db;
    }

    public async Task<List<MealPlan>> GetByDateAsync(string date)
    {
        try
        {
            await _db.Init();
            return await _db.Database.Table<MealPlan>()
                .Where(p => p.PlanDate == date)
                .OrderBy(p => p.MealType)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MealPlanRepository] GetByDateAsync error: {ex.Message}");
            return new();
        }
    }

    public async Task<List<MealPlan>> GetByDateAndMealTypeAsync(string date, string mealType)
    {
        try
        {
            await _db.Init();
            return await _db.Database.Table<MealPlan>()
                .Where(p => p.PlanDate == date && p.MealType == mealType)
                .OrderBy(p => p.MealType)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MealPlanRepository] GetByDateAndMealTypeAsync error: {ex.Message}");
            return new();
        }
    }

    public async Task<DailyNutritionSummary> GetDailySummaryAsync(string date)
    {
        var summary = new DailyNutritionSummary { Date = date };

        try
        {
            await _db.Init();

            var plans = await _db.Database.Table<MealPlan>()
                .Where(p => p.PlanDate == date)
                .ToListAsync();

            // 已摄入：仅统计 IsCompleted=true
            var completed = plans.Where(p => p.IsCompleted).ToList();
            summary.TotalCalories = completed.Sum(p => p.Calories);
            summary.TotalProtein = completed.Sum(p => p.Protein);
            summary.TotalCarbs = completed.Sum(p => p.Carbs);
            summary.TotalFat = completed.Sum(p => p.Fat);
            summary.TotalFiber = completed.Sum(p => p.Fiber);

            // 计划中：统计全部
            summary.PlannedCalories = plans.Sum(p => p.Calories);
            summary.PlannedProtein = plans.Sum(p => p.Protein);
            summary.PlannedCarbs = plans.Sum(p => p.Carbs);
            summary.PlannedFat = plans.Sum(p => p.Fat);
            summary.PlannedFiber = plans.Sum(p => p.Fiber);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MealPlanRepository] GetDailySummaryAsync error: {ex.Message}");
        }

        return summary;
    }

    public async Task<int> SaveAsync(MealPlan plan)
    {
        try
        {
            await _db.Init();
            if (plan.Id > 0)
            {
                return await _db.Database.UpdateAsync(plan);
            }
            else
            {
                plan.CreatedAt = DateTime.Now;
                return await _db.Database.InsertAsync(plan);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MealPlanRepository] SaveAsync error: {ex.Message}");
            return 0;
        }
    }

    public async Task<int> DeleteAsync(MealPlan plan)
    {
        try
        {
            await _db.Init();
            return await _db.Database.DeleteAsync(plan);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MealPlanRepository] DeleteAsync error: {ex.Message}");
            return 0;
        }
    }

    public async Task MarkCompletedAsync(MealPlan plan, bool completed)
    {
        try
        {
            plan.IsCompleted = completed;
            await SaveAsync(plan);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MealPlanRepository] MarkCompletedAsync error: {ex.Message}");
        }
    }

    public async Task<List<string>> GetDatesWithPlansAsync(string startDate, string endDate)
    {
        try
        {
            await _db.Init();
            var all = await _db.Database.Table<MealPlan>()
                .ToListAsync();
            return all.Where(p =>
                string.Compare(p.PlanDate, startDate) >= 0 &&
                string.Compare(p.PlanDate, endDate) <= 0)
                .Select(p => p.PlanDate)
                .Distinct()
                .OrderBy(d => d)
                .ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MealPlanRepository] GetDatesWithPlansAsync error: {ex.Message}");
            return new();
        }
    }
}
