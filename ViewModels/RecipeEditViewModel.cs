using System.Collections.ObjectModel;
using System.Windows.Input;
using ssk.Models;
using ssk.Services;

namespace ssk.ViewModels;

[QueryProperty(nameof(RecipeId), "RecipeId")]
public class RecipeEditViewModel : BaseViewModel
{
    private readonly RecipeRepository _recipeRepository;

    private int _recipeId;
    public int RecipeId
    {
        get => _recipeId;
        set => SetProperty(ref _recipeId, value);
    }

    private string _title = string.Empty;
    public new string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private string _description = string.Empty;
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    private string _selectedCategory = "Chinese";
    public string SelectedCategory
    {
        get => _selectedCategory;
        set => SetProperty(ref _selectedCategory, value);
    }

    private string _selectedDifficulty = "Easy";
    public string SelectedDifficulty
    {
        get => _selectedDifficulty;
        set => SetProperty(ref _selectedDifficulty, value);
    }

    private int _prepTime;
    public int PrepTime
    {
        get => _prepTime;
        set => SetProperty(ref _prepTime, value);
    }

    private int _cookTime;
    public int CookTime
    {
        get => _cookTime;
        set => SetProperty(ref _cookTime, value);
    }

    private int _servings = 1;
    public int Servings
    {
        get => _servings;
        set => SetProperty(ref _servings, value);
    }

    public ObservableCollection<StepModel> Steps { get; } = new();

    private string _tagsInput = string.Empty;
    public string TagsInput
    {
        get => _tagsInput;
        set => SetProperty(ref _tagsInput, value);
    }

    public List<string> Categories { get; } = new() { "Chinese", "Western", "Asian", "Dessert", "Drink" };
    public List<string> Difficulties { get; } = new() { "Easy", "Medium", "Hard" };

    public ICommand AddStepCommand { get; }
    public ICommand RemoveStepCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public RecipeEditViewModel(RecipeRepository recipeRepository)
    {
        _recipeRepository = recipeRepository;
        base.Title = "Edit Recipe";

        AddStepCommand = CreateCommand(ExecuteAddStep);
        RemoveStepCommand = CreateCommand<StepModel>(ExecuteRemoveStep);
        SaveCommand = CreateAsyncCommand(ExecuteSave);
        CancelCommand = CreateCommand(async () =>
            await Shell.Current.GoToAsync(".."));
    }

    public async Task LoadRecipeAsync()
    {
        try
        {
            if (RecipeId <= 0)
            {
                base.Title = "New Recipe";
                return;
            }

            var recipe = await _recipeRepository.GetByIdAsync(RecipeId);
            if (recipe == null) return;

            base.Title = "Edit Recipe";
            Title = recipe.Title;
            Description = recipe.Description ?? string.Empty;
            SelectedCategory = recipe.Category;
            SelectedDifficulty = recipe.Difficulty;
            PrepTime = recipe.PrepTimeMin;
            CookTime = recipe.CookTimeMin;
            Servings = recipe.Servings;

            // parse Instructions JSON
            Steps.Clear();
            try
            {
                var steps = System.Text.Json.JsonSerializer.Deserialize<List<string>>(recipe.Instructions);
                if (steps != null)
                {
                    for (int i = 0; i < steps.Count; i++)
                        Steps.Add(new StepModel(i, steps[i]));
                }
            }
            catch { }

            /* parse Tags */
            try
            {
                if (!string.IsNullOrWhiteSpace(recipe.Tags))
                {
                    var tags = System.Text.Json.JsonSerializer.Deserialize<List<string>>(recipe.Tags);
                    TagsInput = tags != null ? string.Join(",", tags) : string.Empty;
                }
            }
            catch { }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecipeEditViewModel] LoadRecipeAsync error: {ex.Message}");
        }
    }

    private void ExecuteAddStep()
    {
        Steps.Add(new StepModel(Steps.Count, string.Empty));
        ReindexSteps();
    }

    private void ExecuteRemoveStep(StepModel step)
    {
        Steps.Remove(step);
        ReindexSteps();
    }

    private void ReindexSteps()
    {
        for (int i = 0; i < Steps.Count; i++)
            Steps[i].Index = i;
        OnPropertyChanged(nameof(Steps));
    }

    private async Task ExecuteSave()
    {
        try
        {
            // serialize Steps to JSON
            var stepsList = Steps.Select(s => s.Text).ToList();
            var instructionsJson = System.Text.Json.JsonSerializer.Serialize(stepsList);

            /* serialize Tags */
            var tags = TagsInput.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
            var tagsJson = System.Text.Json.JsonSerializer.Serialize(tags);

            var recipe = new Recipe
            {
                Id = RecipeId,
                Title = Title,
                Description = Description,
                Category = SelectedCategory,
                Difficulty = SelectedDifficulty,
                PrepTimeMin = PrepTime,
                CookTimeMin = CookTime,
                Servings = Servings,
                Instructions = instructionsJson,
                Tags = tagsJson,
                IsCustom = true
            };

            if (RecipeId > 0)
            {
                var existing = await _recipeRepository.GetByIdAsync(RecipeId);
                if (existing != null)
                {
                    recipe.ImagePath = existing.ImagePath;
                    recipe.IsFavorite = existing.IsFavorite;
                    recipe.CreatedAt = existing.CreatedAt;
                }
            }

            await _recipeRepository.SaveAsync(recipe);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecipeEditViewModel] Save error: {ex.Message}");
        }
    }
}
