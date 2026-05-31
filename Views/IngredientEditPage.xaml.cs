using ssk.ViewModels;

namespace ssk.Views;

[QueryProperty("IngredientId", "IngredientId")]
public partial class IngredientEditPage : ContentPage
{
    private string? _ingredientId;
    public string? IngredientId
    {
        get => _ingredientId;
        set
        {
            _ingredientId = value;
            if (BindingContext is IngredientEditViewModel vm && !string.IsNullOrEmpty(value) && int.TryParse(value, out int id))
                vm.IngredientId = id;
        }
    }

    public IngredientEditPage(IngredientEditViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        if (BindingContext is IngredientEditViewModel vm)
            await vm.LoadIngredientAsync();
    }
}
