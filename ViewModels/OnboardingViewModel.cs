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

    public string NextButtonText => CurrentStep >= _totalSteps - 1 ? "开始使用" : "下一步";

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
        Items.Add(new OnboardingItem("", "欢迎使用SSK", "SSK智慧饮食管理助手，让每一餐都健康美味"));
        Items.Add(new OnboardingItem("", "智能识别", "拍照即可识别食物，自动计算营养成分，轻松管理饮食计划"));
        Items.Add(new OnboardingItem("", "开始使用", "让我们一起开启健康饮食之旅"));
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
