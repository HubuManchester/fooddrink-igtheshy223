using ssk.ViewModels;

namespace ssk.Views;

public partial class OnboardingPage : ContentPage
{
    private int _currentStep;
    private readonly Services.FirstRunService _firstRunService;

    public OnboardingPage(Services.FirstRunService firstRunService)
    {
        InitializeComponent();
        _firstRunService = firstRunService;
        BindingContext = new ViewModels.OnboardingViewModel(firstRunService);
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        (BindingContext as ViewModels.OnboardingViewModel)?.SkipCommand.Execute(null);
    }

    private void Carousel_PositionChanged(object sender, PositionChangedEventArgs e)
    {
        _currentStep = e.CurrentPosition;
        UpdateIndicators(_currentStep);

        var vm = BindingContext as OnboardingViewModel;
        if (vm != null)
        {
            vm.CurrentStep = _currentStep;
        }
    }

    private void UpdateIndicators(int index)
    {
        var dots = new[] { Dot0, Dot1, Dot2 };
        for (int i = 0; i < dots.Length; i++)
        {
            dots[i].BackgroundColor = i == index
                ? Application.Current!.Resources.TryGetValue("Primary", out var primary) ? (Color)primary! : Color.FromArgb("#6366F1")
                : Application.Current!.RequestedTheme == AppTheme.Light
                    ? Color.FromArgb("#D1D5DB")
                    : Color.FromArgb("#4B5563");
        }
    }
}
