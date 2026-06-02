namespace ssk.Controls;

public partial class NutritionCard : ContentView
{
    public static readonly BindableProperty CaloriesProperty =
        BindableProperty.Create(nameof(Calories), typeof(double), typeof(NutritionCard), 0.0);
    public static readonly BindableProperty ProteinProperty =
        BindableProperty.Create(nameof(Protein), typeof(double), typeof(NutritionCard), 0.0);
    public static readonly BindableProperty CarbsProperty =
        BindableProperty.Create(nameof(Carbs), typeof(double), typeof(NutritionCard), 0.0);
    public static readonly BindableProperty FatProperty =
        BindableProperty.Create(nameof(Fat), typeof(double), typeof(NutritionCard), 0.0);
    public static readonly BindableProperty FiberProperty =
        BindableProperty.Create(nameof(Fiber), typeof(double), typeof(NutritionCard), 0.0);

    public double Calories { get => (double)GetValue(CaloriesProperty); set => SetValue(CaloriesProperty, value); }
    public double Protein { get => (double)GetValue(ProteinProperty); set => SetValue(ProteinProperty, value); }
    public double Carbs { get => (double)GetValue(CarbsProperty); set => SetValue(CarbsProperty, value); }
    public double Fat { get => (double)GetValue(FatProperty); set => SetValue(FatProperty, value); }
    public double Fiber { get => (double)GetValue(FiberProperty); set => SetValue(FiberProperty, value); }

    public NutritionCard()
    {
        InitializeComponent();
    }
}
