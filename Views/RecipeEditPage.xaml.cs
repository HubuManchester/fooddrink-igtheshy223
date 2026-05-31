using ssk.ViewModels;

namespace ssk.Views;

[QueryProperty(nameof(RecipeId), "RecipeId")]
public partial class RecipeEditPage : ContentPage
{
    private readonly RecipeEditViewModel _viewModel;

    public int RecipeId
    {
        get => _viewModel.RecipeId;
        set => _viewModel.RecipeId = value;
    }

    public RecipeEditPage(RecipeEditViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        await _viewModel.LoadRecipeAsync();
    }
}
