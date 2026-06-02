using System.Windows.Input;
using ssk.Models;

namespace ssk.Controls;

public partial class IngredientCard : ContentView
{
    public static readonly BindableProperty IngredientProperty =
        BindableProperty.Create(nameof(Ingredient), typeof(Ingredient), typeof(IngredientCard));

    public static readonly BindableProperty TapCommandProperty =
        BindableProperty.Create(nameof(TapCommand), typeof(ICommand), typeof(IngredientCard));

    public Ingredient? Ingredient
    {
        get => (Ingredient?)GetValue(IngredientProperty);
        set => SetValue(IngredientProperty, value);
    }

    public ICommand? TapCommand
    {
        get => (ICommand?)GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty, value);
    }

    public string CategoryAbbr => Ingredient?.Category switch
    {
        "蔬菜" => "蔬", "水果" => "果", "肉类" => "肉", "海鲜" => "海",
        "蛋奶" => "蛋", "主食" => "主", "调味品" => "调", "调味料" => "调",
        "豆制品" => "豆", _ => "?"
    };

    public IngredientCard()
    {
        InitializeComponent();
    }
}
