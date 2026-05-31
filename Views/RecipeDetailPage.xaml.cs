using ssk.ViewModels;

namespace ssk.Views;

[QueryProperty(nameof(RecipeId), "RecipeId")]
public partial class RecipeDetailPage : ContentPage
{
    private readonly RecipeDetailViewModel _viewModel;

    public int RecipeId
    {
        get => _viewModel.RecipeId;
        set => _viewModel.RecipeId = value;
    }

    public RecipeDetailPage(RecipeDetailViewModel viewModel)
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

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        // 页面离开时停止朗读
        if (_viewModel.IsSpeaking)
        {
            _viewModel.ToggleSpeakingCommand.Execute(null);
        }
    }
}
