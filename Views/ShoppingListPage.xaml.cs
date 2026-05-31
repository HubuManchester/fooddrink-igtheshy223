using ssk.ViewModels;

namespace ssk.Views;

public partial class ShoppingListPage : ContentPage
{
    public ShoppingListPage(ShoppingListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        if (BindingContext is ShoppingListViewModel vm)
            await vm.LoadItemsAsync();
    }
}
