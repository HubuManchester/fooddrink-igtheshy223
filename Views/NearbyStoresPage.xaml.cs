using ssk.ViewModels;

namespace ssk.Views;

public partial class NearbyStoresPage : ContentPage
{
    public NearbyStoresPage(NearbyStoresViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        if (BindingContext is NearbyStoresViewModel vm)
            await vm.InitializeAsync();
    }
}
