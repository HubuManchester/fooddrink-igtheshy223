using System.Collections.ObjectModel;
using System.Windows.Input;
using ssk.Models;
using ssk.Services;

namespace ssk.ViewModels;

public class RecipeListViewModel : BaseViewModel
{
    private readonly RecipeRepository _recipeRepository;

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private string _selectedCategory = "All";
    public string SelectedCategory
    {
        get => _selectedCategory;
        set => SetProperty(ref _selectedCategory, value);
    }

    public ObservableCollection<Recipe> Recipes { get; } = new();

    public List<string> Categories { get; } = new() { "All", "Chinese", "Western", "Asian", "Dessert", "Drink" };

    public ICommand SearchCommand { get; }
    public ICommand SelectCategoryCommand { get; }
    public ICommand NavigateToDetailCommand { get; }
    public ICommand NavigateToAddCommand { get; }
    public ICommand ToggleFavoriteCommand { get; }

    private bool _isRefreshing;
    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
    }

    public ICommand RefreshCommand { get; }

    public RecipeListViewModel(RecipeRepository recipeRepository)
    {
        _recipeRepository = recipeRepository;
        Title = "Recipe List";

        SearchCommand = CreateAsyncCommand(ExecuteSearch);
        SelectCategoryCommand = CreateAsyncCommand<string>(ExecuteSelectCategory);
        NavigateToDetailCommand = CreateAsyncCommand<Recipe>(ExecuteNavigateToDetail);
        NavigateToAddCommand = CreateCommand(() =>
            Shell.Current.GoToAsync("RecipeEditPage"));
        ToggleFavoriteCommand = CreateAsyncCommand<Recipe>(ExecuteToggleFavorite);

        RefreshCommand = new Command(async () =>
        {
            try
            {
                await LoadRecipesAsync();
            }
            finally
            {
                IsRefreshing = false;
            }
        });
    }

    public async Task LoadRecipesAsync()
    {
        try
        {
            List<Recipe> recipes;

            if (SelectedCategory == "All")
            {
                if (!string.IsNullOrWhiteSpace(SearchText))
                    recipes = await _recipeRepository.SearchAsync(SearchText);
                else
                    recipes = await _recipeRepository.GetAllAsync();
            }
            else
            {
                recipes = await _recipeRepository.GetByCategoryAsync(SelectedCategory);
                if (!string.IsNullOrWhiteSpace(SearchText))
                    recipes = recipes.Where(r =>
                        r.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        (r.Description != null && r.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)))
                        .ToList();
            }

            Recipes.Clear();
            foreach (var recipe in recipes)
                Recipes.Add(recipe);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecipeListViewModel] LoadRecipesAsync error: {ex.Message}");
        }
    }

    private async Task ExecuteSearch()
    {
        await LoadRecipesAsync();
    }

    private async Task ExecuteSelectCategory(string category)
    {
        SelectedCategory = category;
        await LoadRecipesAsync();
    }

    private async Task ExecuteNavigateToDetail(Recipe recipe)
    {
        await Shell.Current.GoToAsync($"RecipeDetailPage?RecipeId={recipe.Id}");
    }

    private async Task ExecuteToggleFavorite(Recipe recipe)
    {
        try
        {
            await _recipeRepository.ToggleFavoriteAsync(recipe);
            await LoadRecipesAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecipeListViewModel] ToggleFavorite error: {ex.Message}");
        }
    }
}
