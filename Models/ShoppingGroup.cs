namespace ssk.Models;

public class ShoppingGroup
{
    public string Category { get; set; } = string.Empty;
    public List<ShoppingItem> Items { get; set; } = new();

    public ShoppingGroup() { }

    public ShoppingGroup(string category, List<ShoppingItem> items)
    {
        Category = category;
        Items = items;
    }
}
