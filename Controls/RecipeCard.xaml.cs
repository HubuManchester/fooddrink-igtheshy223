using System.Windows.Input;
using ssk.Models;

namespace ssk.Controls;

public partial class RecipeCard : ContentView
{
    public static readonly BindableProperty RecipeProperty =
        BindableProperty.Create(nameof(Recipe), typeof(Recipe), typeof(RecipeCard));

    public static readonly BindableProperty FavoriteCommandProperty =
        BindableProperty.Create(nameof(FavoriteCommand), typeof(ICommand), typeof(RecipeCard));

    public Recipe? Recipe
    {
        get => (Recipe?)GetValue(RecipeProperty);
        set => SetValue(RecipeProperty, value);
    }

    public ICommand? FavoriteCommand
    {
        get => (ICommand?)GetValue(FavoriteCommandProperty);
        set => SetValue(FavoriteCommandProperty, value);
    }

    public RecipeCard()
    {
        InitializeComponent();
    }
}
