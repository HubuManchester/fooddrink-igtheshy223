namespace ssk.Controls;

public partial class ExpiryBadge : ContentView
{
    public static readonly BindableProperty ExpiryDateProperty =
        BindableProperty.Create(nameof(ExpiryDate), typeof(DateTime?), typeof(ExpiryBadge), null);

    public DateTime? ExpiryDate
    {
        get => (DateTime?)GetValue(ExpiryDateProperty);
        set => SetValue(ExpiryDateProperty, value);
    }

    public ExpiryBadge()
    {
        InitializeComponent();
    }
}
