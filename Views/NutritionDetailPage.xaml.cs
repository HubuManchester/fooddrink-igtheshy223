using ssk.ViewModels;

namespace ssk.Views;

public partial class NutritionDetailPage : ContentPage
{
    public NutritionDetailPage(NutritionDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        if (BindingContext is NutritionDetailViewModel vm)
            await vm.LoadDataAsync();
    }
}
