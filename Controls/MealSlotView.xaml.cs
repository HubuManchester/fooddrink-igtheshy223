using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using ssk.Models;

namespace ssk.Controls;

public partial class MealSlotView : ContentView
{
    private ObservableCollection<MealPlan>? _trackedCollection;

    public static readonly BindableProperty MealTypeProperty =
        BindableProperty.Create(nameof(MealType), typeof(string), typeof(MealSlotView), "");
    public static readonly BindableProperty PlansProperty =
        BindableProperty.Create(nameof(Plans), typeof(ObservableCollection<MealPlan>), typeof(MealSlotView), new ObservableCollection<MealPlan>(), propertyChanged: OnPlansPropertyChanged);
    public static readonly BindableProperty ToggleCommandProperty =
        BindableProperty.Create(nameof(ToggleCommand), typeof(ICommand), typeof(MealSlotView));

    public string MealType { get => (string)GetValue(MealTypeProperty); set => SetValue(MealTypeProperty, value); }
    public ObservableCollection<MealPlan> Plans { get => (ObservableCollection<MealPlan>)GetValue(PlansProperty); set => SetValue(PlansProperty, value); }
    public ICommand? ToggleCommand { get => (ICommand?)GetValue(ToggleCommandProperty); set => SetValue(ToggleCommandProperty, value); }

    public int PlanCount => Plans?.Count ?? 0;
    public bool HasPlans => Plans?.Count > 0;
    public bool NoPlans => Plans == null || Plans.Count == 0;

    public MealSlotView()
    {
        InitializeComponent();
    }

    private static void OnPlansPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var ctrl = (MealSlotView)bindable;
        ctrl.TrackCollection((ObservableCollection<MealPlan>?)oldValue, (ObservableCollection<MealPlan>?)newValue);
        ctrl.NotifyPlanPropertiesChanged();
    }

    private void TrackCollection(ObservableCollection<MealPlan>? oldCollection, ObservableCollection<MealPlan>? newCollection)
    {
        if (oldCollection != null)
            oldCollection.CollectionChanged -= OnPlansCollectionChanged;
        if (newCollection != null)
            newCollection.CollectionChanged += OnPlansCollectionChanged;
        _trackedCollection = newCollection;
    }

    private void OnPlansCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        NotifyPlanPropertiesChanged();
    }

    private void NotifyPlanPropertiesChanged()
    {
        OnPropertyChanged(nameof(PlanCount));
        OnPropertyChanged(nameof(HasPlans));
        OnPropertyChanged(nameof(NoPlans));
    }

    private void OnCheckBoxCheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox cb && cb.BindingContext is MealPlan plan && ToggleCommand?.CanExecute(plan) == true)
            ToggleCommand.Execute(plan);
    }
}
