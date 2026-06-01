using System.Collections.ObjectModel;
using System.Windows.Input;
using ssk.Models;
using ssk.Services;
using ssk.Views;

namespace ssk.ViewModels;

public class IngredientListViewModel : BaseViewModel
{
    private readonly IngredientRepository _ingredientRepository;
    private readonly AuthService _authService;

    private string _selectedCategory = "全部";
    public string SelectedCategory
    {
        get => _selectedCategory;
        set => SetProperty(ref _selectedCategory, value);
    }

    public ObservableCollection<Ingredient> Ingredients { get; } = new();

    public List<string> Categories { get; } = new() { "全部", "蔬菜", "水果", "肉类", "海鲜", "蛋奶", "主食", "调味品" };

    public ICommand SelectCategoryCommand { get; }
    public ICommand NavigateToEditCommand { get; }
    public ICommand NavigateToAddCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand LoginCommand { get; }

    private bool _isRefreshing;
    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
    }

    public ICommand RefreshCommand { get; }

    public bool IsLoggedIn => _authService.IsLoggedIn;

    public IngredientListViewModel(IngredientRepository ingredientRepository, AuthService authService)
    {
        _ingredientRepository = ingredientRepository;
        _authService = authService;
        Title = "食材管理";

        _authService.LoginStateChanged += OnLoginStateChanged;

        SelectCategoryCommand = CreateAsyncCommand<string>(ExecuteSelectCategory);
        NavigateToEditCommand = CreateCommand<Ingredient>(ingredient =>
        {
            if (ingredient != null)
                Shell.Current.GoToAsync($"IngredientEditPage?IngredientId={ingredient.Id}");
        });
        NavigateToAddCommand = CreateCommand(() =>
            Shell.Current.GoToAsync("IngredientEditPage"));
        DeleteCommand = CreateAsyncCommand<Ingredient>(ExecuteDelete);
        LoginCommand = CreateCommand(() =>
            Shell.Current.GoToAsync(nameof(LoginPage)));

        RefreshCommand = new Command(async () =>
        {
            try
            {
                await LoadIngredientsAsync();
            }
            finally
            {
                IsRefreshing = false;
            }
        });
    }

    public async Task LoadIngredientsAsync()
    {
        try
        {
            List<Ingredient> ingredients;

            if (SelectedCategory == "全部")
                ingredients = await _ingredientRepository.GetAllAsync();
            else
                ingredients = await _ingredientRepository.GetByCategoryAsync(SelectedCategory);

            Ingredients.Clear();
            foreach (var ingredient in ingredients)
                Ingredients.Add(ingredient);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[IngredientListViewModel] LoadIngredientsAsync error: {ex.Message}");
        }
    }

    private async Task ExecuteSelectCategory(string category)
    {
        SelectedCategory = category;
        await LoadIngredientsAsync();
    }

    private async Task ExecuteDelete(Ingredient ingredient)
    {
        try
        {
            var confirm = await Shell.Current.DisplayAlert("确认删除", $"确定要删除食材\"{ingredient.Name}\"吗？", "删除", "取消");
            if (!confirm) return;

            await _ingredientRepository.DeleteAsync(ingredient);
            await LoadIngredientsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[IngredientListViewModel] Delete error: {ex.Message}");
        }
    }

    private void OnLoginStateChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            OnPropertyChanged(nameof(IsLoggedIn));
            if (_authService.IsLoggedIn)
                _ = LoadIngredientsAsync();
        });
    }
}
