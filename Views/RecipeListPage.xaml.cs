using ssk.Helpers;
using ssk.ViewModels;

namespace ssk.Views;

public partial class RecipeListPage : ContentPage
{
    private readonly RecipeListViewModel _viewModel;

    public RecipeListPage(RecipeListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
        BuildCategoryButtons();
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void BuildCategoryButtons()
    {
        CategoryLayout.Children.Clear();
        foreach (var category in _viewModel.Categories)
        {
            var btn = new Button
            {
                Text = category,
                FontSize = 13,
                Padding = new Thickness(16, 8),
                CornerRadius = 20,
                MinimumHeightRequest = 36,
                Command = _viewModel.SelectCategoryCommand,
                CommandParameter = category,
                AutomationId = $"Cat_{category}"
            };
            SemanticProperties.SetHint(btn, category);

            UpdateCategoryButtonStyle(btn, category == _viewModel.SelectedCategory);
            CategoryLayout.Children.Add(btn);
        }
    }

    private void UpdateCategoryButtonStyle(Button btn, bool isSelected)
    {
        if (isSelected)
        {
            btn.BackgroundColor = (Color)Application.Current.Resources["Primary"];
            btn.TextColor = Colors.White;
            btn.BorderWidth = 0;
        }
        else
        {
            btn.BackgroundColor = Colors.Transparent;
            btn.TextColor = Application.Current.RequestedTheme == AppTheme.Light
                ? Color.FromArgb("#374151")
                : Color.FromArgb("#D1D5DB");
            btn.BorderColor = Application.Current.RequestedTheme == AppTheme.Light
                ? Color.FromArgb("#D1D5DB")
                : Color.FromArgb("#4B5563");
            btn.BorderWidth = 1;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(RecipeListViewModel.SelectedCategory))
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                foreach (var child in CategoryLayout.Children)
                {
                    if (child is Button btn)
                    {
                        UpdateCategoryButtonStyle(btn, btn.Text == _viewModel.SelectedCategory);
                    }
                }
            });
        }
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        await _viewModel.LoadRecipesAsync();
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (RecipeGridLayout != null && AdaptiveHelper.IsTablet)
        {
            RecipeGridLayout.Span = AdaptiveHelper.ComputeSpan(width - 32, 180);
        }
    }
}
