using System.Collections.ObjectModel;
using System.Windows.Input;
using ssk.Models;
using ssk.Services;

namespace ssk.ViewModels;

[QueryProperty(nameof(RecipeId), "RecipeId")]
public class RecipeDetailViewModel : BaseViewModel
{
    private readonly RecipeRepository _recipeRepository;
    private readonly MealPlanRepository _mealPlanRepository;
    private readonly TextToSpeechService _ttsService;

    private int _recipeId;
    public int RecipeId
    {
        get => _recipeId;
        set => SetProperty(ref _recipeId, value);
    }

    private Recipe? _currentRecipe;
    public Recipe? CurrentRecipe
    {
        get => _currentRecipe;
        set => SetProperty(ref _currentRecipe, value);
    }

    public ObservableCollection<StepModel> Steps { get; } = new();
    public List<string> TagList { get; private set; } = new();

    private NutritionInfo _nutrition = NutritionInfo.Empty;
    public NutritionInfo Nutrition
    {
        get => _nutrition;
        set => SetProperty(ref _nutrition, value);
    }

    private bool _isSpeaking;
    public bool IsSpeaking
    {
        get => _isSpeaking;
        set
        {
            if (SetProperty(ref _isSpeaking, value))
                OnPropertyChanged(nameof(SpeakButtonText));
        }
    }

    public string SpeakButtonText => IsSpeaking ? "Stop Read" : "Read Steps";

    public ICommand ToggleFavoriteCommand { get; }
    public ICommand ToggleSpeakingCommand { get; }
    public ICommand AddToPlanCommand { get; }
    public ICommand GoBackCommand { get; }

    public RecipeDetailViewModel(
        RecipeRepository recipeRepository,
        MealPlanRepository mealPlanRepository,
        TextToSpeechService ttsService)
    {
        _recipeRepository = recipeRepository;
        _mealPlanRepository = mealPlanRepository;
        _ttsService = ttsService;

        Title = "Recipe Detail";

        ToggleFavoriteCommand = CreateAsyncCommand(ExecuteToggleFavorite);
        ToggleSpeakingCommand = new Command(async () => await ExecuteToggleSpeaking(), () => !IsBusy);
        AddToPlanCommand = CreateAsyncCommand(ExecuteAddToPlan);
        GoBackCommand = CreateCommand(async () =>
        {
            _ttsService.Stop();
            await Shell.Current.GoToAsync("..");
        });

        _ttsService.SpeakingStateChanged += OnSpeakingStateChanged;
    }

    public async Task LoadRecipeAsync()
    {
        try
        {
            if (RecipeId <= 0) return;

            CurrentRecipe = await _recipeRepository.GetByIdAsync(RecipeId);
            if (CurrentRecipe == null) return;

            Title = CurrentRecipe.Title;

            // parse Instructions JSON
            ParseSteps(CurrentRecipe.Instructions);

            /* parse Tags JSON */
            ParseTags(CurrentRecipe.Tags);

            // nutrition estimate calculate
            Nutrition = NutritionCalculator.CalculateForRecipe(CurrentRecipe);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecipeDetailViewModel] LoadRecipeAsync error: {ex.Message}");
        }
    }

    private void ParseSteps(string instructionsJson)
    {
        Steps.Clear();
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(instructionsJson);
            var root = doc.RootElement;
            if (root.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                int i = 0;
                foreach (var element in root.EnumerateArray())
                {
                    string text;
                    if (element.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        text = element.GetString() ?? "";
                    }
                    else if (element.TryGetProperty("Text", out var textProp))
                    {
                        text = textProp.GetString() ?? "";
                    }
                    else
                    {
                        text = element.GetRawText();
                    }
                    Steps.Add(new StepModel(i, text));
                    i++;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecipeDetailViewModel] ParseSteps error: {ex.Message}");
        }
    }

    private void ParseTags(string? tagsJson)
    {
        TagList = new List<string>();
        try
        {
            if (!string.IsNullOrWhiteSpace(tagsJson))
            {
                var tags = System.Text.Json.JsonSerializer.Deserialize<List<string>>(tagsJson);
                if (tags != null)
                    TagList = tags;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecipeDetailViewModel] ParseTags error: {ex.Message}");
        }
        OnPropertyChanged(nameof(TagList));
    }

    private async Task ExecuteToggleFavorite()
    {
        try
        {
            if (CurrentRecipe == null) return;
            await _recipeRepository.ToggleFavoriteAsync(CurrentRecipe);
            OnPropertyChanged(nameof(CurrentRecipe));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecipeDetailViewModel] ToggleFavorite error: {ex.Message}");
        }
    }

    private async Task ExecuteToggleSpeaking()
    {
        try
        {
            if (IsSpeaking)
            {
                _ttsService.Stop();
                IsSpeaking = false;
            }
            else
            {
                IsSpeaking = true;
                await _ttsService.SpeakStepsAsync(Steps);
                IsSpeaking = false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecipeDetailViewModel] ToggleSpeaking error: {ex.Message}");
            IsSpeaking = false;
        }
    }

    private async Task ExecuteAddToPlan()
    {
        try
        {
            if (CurrentRecipe == null) return;
            await Shell.Current.GoToAsync($"MealPlanPage?action=add&recipeId={CurrentRecipe.Id}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecipeDetailViewModel] AddToPlan error: {ex.Message}");
        }
    }

    private void OnSpeakingStateChanged(bool isSpeaking)
    {
        MainThread.BeginInvokeOnMainThread(() => IsSpeaking = isSpeaking);
    }
}
