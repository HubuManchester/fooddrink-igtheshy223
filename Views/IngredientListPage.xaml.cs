using ssk.Helpers;
using ssk.ViewModels;

namespace ssk.Views;

public partial class IngredientListPage : ContentPage
{
    public IngredientListPage(IngredientListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        if (BindingContext is IngredientListViewModel vm)
            await vm.LoadIngredientsAsync();
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (IngredientGridLayout != null && AdaptiveHelper.IsTablet)
        {
            IngredientGridLayout.Span = AdaptiveHelper.ComputeSpan(width - 32, 280);
        }
    }
}
