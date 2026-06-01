using System.Windows.Input;
using ssk.Models;
using ssk.Services;

namespace ssk.ViewModels;

[QueryProperty(nameof(IngredientId), "IngredientId")]
public class IngredientEditViewModel : BaseViewModel
{
    private readonly IngredientRepository _ingredientRepository;

    private int _ingredientId;
    public int IngredientId
    {
        get => _ingredientId;
        set => SetProperty(ref _ingredientId, value);
    }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string _selectedCategory = "蔬菜";
    public string SelectedCategory
    {
        get => _selectedCategory;
        set => SetProperty(ref _selectedCategory, value);
    }

    private double _quantity;
    public double Quantity
    {
        get => _quantity;
        set => SetProperty(ref _quantity, value);
    }

    private string _selectedUnit = "克";
    public string SelectedUnit
    {
        get => _selectedUnit;
        set => SetProperty(ref _selectedUnit, value);
    }

    private DateTime _purchaseDate = DateTime.Today;
    public DateTime PurchaseDate
    {
        get => _purchaseDate;
        set => SetProperty(ref _purchaseDate, value);
    }

    private DateTime _expiryDate = DateTime.Today.AddDays(7);
    public DateTime ExpiryDate
    {
        get => _expiryDate;
        set => SetProperty(ref _expiryDate, value);
    }

    private string _selectedStorage = "冰箱";
    public string SelectedStorage
    {
        get => _selectedStorage;
        set => SetProperty(ref _selectedStorage, value);
    }

    public List<string> Categories { get; } = new() { "蔬菜", "水果", "肉类", "海鲜", "蛋奶", "主食", "调味品" };
    public List<string> Units { get; } = new() { "克", "千克", "个", "根", "块", "片", "毫升", "升", "勺" };
    public List<string> StorageLocations { get; } = new() { "冰箱", "冷冻室", "常温", "阴凉处" };

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public IngredientEditViewModel(IngredientRepository ingredientRepository)
    {
        _ingredientRepository = ingredientRepository;
        Title = "编辑食材";

        SaveCommand = CreateAsyncCommand(ExecuteSave);
        CancelCommand = CreateCommand(async () =>
            await Shell.Current.GoToAsync(".."));
    }

    public async Task LoadIngredientAsync()
    {
        try
        {
            if (IngredientId <= 0)
            {
                Title = "添加食材";
                return;
            }

            var ingredient = await _ingredientRepository.GetByIdAsync(IngredientId);
            if (ingredient == null) return;

            Title = "编辑食材";
            Name = ingredient.Name;
            SelectedCategory = ingredient.Category;
            Quantity = ingredient.Quantity;
            SelectedUnit = ingredient.DefaultUnit;
            PurchaseDate = ingredient.PurchaseDate ?? DateTime.Today;
            ExpiryDate = ingredient.ExpiryDate ?? DateTime.Today.AddDays(7);
            SelectedStorage = ingredient.StorageLocation;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[IngredientEditViewModel] LoadIngredientAsync error: {ex.Message}");
        }
    }

    private async Task ExecuteSave()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                await Shell.Current.DisplayAlert("提示", "请输入食材名称", "确定");
                return;
            }

            var ingredient = new Ingredient
            {
                Id = IngredientId,
                Name = Name,
                Category = SelectedCategory,
                DefaultUnit = SelectedUnit,
                Quantity = Quantity,
                PurchaseDate = PurchaseDate,
                ExpiryDate = ExpiryDate,
                StorageLocation = SelectedStorage
            };

            if (IngredientId > 0)
            {
                var existing = await _ingredientRepository.GetByIdAsync(IngredientId);
                if (existing != null)
                {
                    ingredient.ImagePath = existing.ImagePath;
                    ingredient.NutritionPer100g = existing.NutritionPer100g;
                    ingredient.YoloLabel = existing.YoloLabel;
                    ingredient.CreatedAt = existing.CreatedAt;
                }
            }

            await _ingredientRepository.SaveAsync(ingredient);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[IngredientEditViewModel] Save error: {ex.Message}");
        }
    }
}
