using System.Windows.Input;
using ssk.Models;
using ssk.Services;

namespace ssk.ViewModels;

public class CameraViewModel : BaseViewModel
{
    private readonly CameraService _cameraService;
    private readonly YoloInferenceService _yoloService;
    private readonly IngredientRepository _ingredientRepository;
    private readonly MealPlanRepository _mealPlanRepository;

    private ImageSource? _photoImage;
    public ImageSource? PhotoImage
    {
        get => _photoImage;
        set
        {
            if (SetProperty(ref _photoImage, value))
                OnPropertyChanged(nameof(HasPhoto));
        }
    }

    public bool HasPhoto => PhotoImage != null;

    private string? _photoPath;
    public string? PhotoPath
    {
        get => _photoPath;
        set => SetProperty(ref _photoPath, value);
    }

    private bool _isProcessing;
    public bool IsProcessing
    {
        get => _isProcessing;
        set => SetProperty(ref _isProcessing, value);
    }

    private YoloPrediction? _topPrediction;
    public YoloPrediction? TopPrediction
    {
        get => _topPrediction;
        set => SetProperty(ref _topPrediction, value);
    }

    private string? _recognizedFoodName;
    public string? RecognizedFoodName
    {
        get => _recognizedFoodName;
        set => SetProperty(ref _recognizedFoodName, value);
    }

    private float _confidence;
    public float Confidence
    {
        get => _confidence;
        set => SetProperty(ref _confidence, value);
    }

    private NutritionInfo _nutrition = NutritionInfo.Empty;
    public NutritionInfo Nutrition
    {
        get => _nutrition;
        set => SetProperty(ref _nutrition, value);
    }

    private bool _hasResult;
    public bool HasResult
    {
        get => _hasResult;
        set => SetProperty(ref _hasResult, value);
    }

    private bool _flashOn;
    public bool FlashOn
    {
        get => _flashOn;
        set => SetProperty(ref _flashOn, value);
    }

    public ICommand CaptureCommand { get; }
    public ICommand PickPhotoCommand { get; }
    public ICommand ToggleFlashCommand { get; }
    public ICommand AddToPlanCommand { get; }
    public ICommand ResetCommand { get; }

    public CameraViewModel(
        CameraService cameraService,
        YoloInferenceService yoloService,
        IngredientRepository ingredientRepository,
        MealPlanRepository mealPlanRepository)
    {
        _cameraService = cameraService;
        _yoloService = yoloService;
        _ingredientRepository = ingredientRepository;
        _mealPlanRepository = mealPlanRepository;
        Title = "Photo Recognize";

        CaptureCommand = CreateAsyncCommand(ExecuteCapture);
        PickPhotoCommand = CreateAsyncCommand(ExecutePickPhoto);
        ToggleFlashCommand = CreateAsyncCommand(ExecuteToggleFlash);
        AddToPlanCommand = CreateAsyncCommand(ExecuteAddToPlan);
        ResetCommand = CreateCommand(ExecuteReset);
    }

    private async Task ExecuteCapture()
    {
        try
        {
            var result = await _cameraService.CapturePhotoAsync();
            if (result == null) return;

            var localPath = await _cameraService.SaveToLocalAsync(result);
            await ProcessImageAsync(localPath);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CameraViewModel] Capture error: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", "Photo capture fail, please retry", "OK");
        }
    }

    private async Task ExecutePickPhoto()
    {
        try
        {
            var result = await _cameraService.PickPhotoAsync();
            if (result == null) return;

            var localPath = await _cameraService.SaveToLocalAsync(result);
            await ProcessImageAsync(localPath);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CameraViewModel] PickPhoto error: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", "Select photo fail, please retry", "OK");
        }
    }

    private async Task ProcessImageAsync(string imagePath)
    {
        IsProcessing = true;
        HasResult = false;

        try
        {
            PhotoPath = imagePath;
            PhotoImage = ImageSource.FromFile(imagePath);

            /* yolo model do recognize */
            var predictions = await _yoloService.PredictAsync(imagePath);

            if (predictions.Count > 0)
            {
                // pick highest confidence result
                TopPrediction = predictions.OrderByDescending(p => p.Confidence).First();
                Confidence = TopPrediction.Confidence;

                // map to chinese food name
                RecognizedFoodName = YoloInferenceService.MapLabelToChinese(TopPrediction.Label)
                    ?? TopPrediction.Label;

                /* try get nutrition info from ingredient library */
                var ingredients = await _ingredientRepository.GetByYoloLabelAsync(TopPrediction.Label);
                if (ingredients.Count > 0 && !string.IsNullOrEmpty(ingredients[0].NutritionPer100g))
                {
                    Nutrition = NutritionCalculator.CalculateForIngredient(ingredients[0]);
                }
                else
                {
                    Nutrition = NutritionInfo.Empty;
                }

                HasResult = true;
            }
            else
            {
                RecognizedFoodName = null;
                TopPrediction = null;
                Confidence = 0;
                Nutrition = NutritionInfo.Empty;
                HasResult = false;
                await Shell.Current.DisplayAlert("Hint", "Cannot recognize food, please take photo again", "OK");
            }
        }
        catch (InvalidOperationException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CameraViewModel] Model error: {ex.Message}");
            await Shell.Current.DisplayAlert("Model Not Ready", ex.Message, "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CameraViewModel] ProcessImage error: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", "Recognize fail, please retry", "OK");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private async Task ExecuteToggleFlash()
    {
        try
        {
            FlashOn = !FlashOn;
            await _cameraService.ToggleFlashAsync(FlashOn);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CameraViewModel] ToggleFlash error: {ex.Message}");
            FlashOn = false;
        }
    }

    private async Task ExecuteAddToPlan()
    {
        try
        {
            if (!HasResult || string.IsNullOrEmpty(RecognizedFoodName)) return;

            var mealType = "Snack";
            var dateStr = DateTime.Today.ToString("yyyy-MM-dd");

            var plan = new MealPlan
            {
                PlanDate = dateStr,
                MealType = mealType,
                CustomFoodName = RecognizedFoodName,
                Servings = 1,
                Calories = Nutrition.Calories,
                Protein = Nutrition.Protein,
                Carbs = Nutrition.Carbs,
                Fat = Nutrition.Fat,
                Fiber = Nutrition.Fiber
            };

            await _mealPlanRepository.SaveAsync(plan);
            await Shell.Current.DisplayAlert("Success", $"Already add \"{RecognizedFoodName}\" to today diet plan", "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CameraViewModel] AddToPlan error: {ex.Message}");
        }
    }

    private void ExecuteReset()
    {
        PhotoImage = null;
        PhotoPath = null;
        TopPrediction = null;
        RecognizedFoodName = null;
        Confidence = 0;
        Nutrition = NutritionInfo.Empty;
        HasResult = false;
        IsProcessing = false;
    }
}
