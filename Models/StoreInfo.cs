namespace ssk.Models;

public class StoreInfo
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Distance { get; set; }
    public string Type { get; set; } = string.Empty;
    public string DistanceText => $"{Distance:F1}km";
    public string TypeAbbr => Type switch
    {
        "Supermarket" => "Sup", "Convenience" => "Con", "Market" => "Mkt", "Fruit" => "Fru",
        "Fresh" => "Fre", "Warehouse" => "Wh", _ => "Store"
    };
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string CoordinatesText => $"Lat{Latitude:F6} Lon{Longitude:F6}";
    public string CoordinatesDisplay => CoordinatesText;

    public StoreInfo() { }

    public StoreInfo(string name, string address, double distance,
        string type, double latitude, double longitude)
    {
        Name = name;
        Address = address;
        Distance = distance;
        Type = type;
        Latitude = latitude;
        Longitude = longitude;
    }
}
