using ssk.ViewModels;

namespace ssk.Views;

[QueryProperty("Action", "action")]
[QueryProperty("RecipeIdParam", "recipeId")]
[QueryProperty("MealTypeParam", "mealType")]
public partial class MealPlanPage : ContentPage
{
    private string? _action;
    public string? Action
    {
        get => _action;
        set
        {
            _action = value;
            if (BindingContext is MealPlanViewModel vm && !string.IsNullOrEmpty(value))
                vm.Action = value;
        }
    }

    private string? _recipeIdParam;
    public string? RecipeIdParam
    {
        get => _recipeIdParam;
        set
        {
            _recipeIdParam = value;
            if (BindingContext is MealPlanViewModel vm && !string.IsNullOrEmpty(value))
                vm.RecipeIdParam = value;
        }
    }

    private string? _mealTypeParam;
    public string? MealTypeParam
    {
        get => _mealTypeParam;
        set
        {
            _mealTypeParam = value;
            if (BindingContext is MealPlanViewModel vm && !string.IsNullOrEmpty(value))
                vm.MealTypeParam = value;
        }
    }

    public MealPlanPage(MealPlanViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        if (BindingContext is MealPlanViewModel vm)
        {
            await vm.HandleNavigationParametersAsync();
            await vm.LoadDayPlansAsync();
        }
    }
}
