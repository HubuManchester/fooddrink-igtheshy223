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
        "超市" => "超", "便利" => "便", "菜场" => "菜", "水果" => "果",
        "生鲜" => "鲜", "仓储" => "仓", _ => "店"
    };
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string CoordinatesText => $"纬度{Latitude:F6} 经度{Longitude:F6}";
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
