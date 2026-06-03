using System.Collections.ObjectModel;
using System.Windows.Input;
using ssk.Services;
using ssk.Views;

namespace ssk.ViewModels;

public class OnboardingItem
{
    public string Icon { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public OnboardingItem() { }

    public OnboardingItem(string icon, string title, string description)
    {
        Icon = icon;
        Title = title;
        Description = description;
    }
}

public class OnboardingViewModel : BaseViewModel
{
    private readonly FirstRunService _firstRunService;

    public ObservableCollection<OnboardingItem> Items { get; } = new();

    private int _currentStep;
    public int CurrentStep
    {
        get => _currentStep;
        set
        {
            if (SetProperty(ref _currentStep, value))
                OnPropertyChanged(nameof(NextButtonText));
        }
    }

    private int _totalSteps = 3;

    public string NextButtonText => CurrentStep >= _totalSteps - 1 ? "Start Use" : "Next Step";

    public ICommand NextCommand { get; }
    public ICommand SkipCommand { get; }

    public OnboardingViewModel(FirstRunService firstRunService)
    {
        _firstRunService = firstRunService;

        NextCommand = new Command(ExecuteNext);
        SkipCommand = new Command(async () => await ExecuteSkip());

        LoadItems();
    }

    private void LoadItems()
    {
        Items.Add(new OnboardingItem("", "Welcome Use SSK", "SSK smart diet management assistant, make every meal healthy delicious"));
        Items.Add(new OnboardingItem("", "Smart Recognize", "Take photo can recognize food, auto calculate nutrition, easy manage diet plan"));
        Items.Add(new OnboardingItem("", "Start Use", "Let us together start healthy diet journey"));
    }

    private void ExecuteNext()
    {
        if (CurrentStep < _totalSteps - 1)
        {
            CurrentStep++;
        }
        else
        {
            _ = ExecuteSkip();
        }
    }

    private async Task ExecuteSkip()
    {
        try
        {
            await _firstRunService.MarkCompletedAsync();
            await Shell.Current.GoToAsync(nameof(HomePage));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[OnboardingViewModel] Skip error: {ex.Message}");
        }
    }
}
