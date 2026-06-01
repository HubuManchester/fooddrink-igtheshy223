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

    public string SpeakButtonText => IsSpeaking ? "停止朗读" : "朗读步骤";

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

        Title = "食谱详情";

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

            // 解析 Instructions JSON
            ParseSteps(CurrentRecipe.Instructions);

            // 解析 Tags JSON
            ParseTags(CurrentRecipe.Tags);

            // 营养估算
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
            var steps = System.Text.Json.JsonSerializer.Deserialize<List<string>>(instructionsJson);
            if (steps != null)
            {
                for (int i = 0; i < steps.Count; i++)
                    Steps.Add(new StepModel(i, steps[i]));
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
