using ssk.Models;

namespace ssk.Services;

public class GeolocationService
{
    private readonly CompassService _compassService;
    private Location? _currentLocation;

    public Location? CurrentLocation => _currentLocation;
    public string LocationStatus { get; private set; } = "Getting location...";

    public event Action? LocationUpdated;

    public GeolocationService(CompassService compassService)
    {
        _compassService = compassService;
    }

    public async Task RefreshLocationAsync()
    {
        try
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                // fallback default location when no permission
                _currentLocation = new Location(39.9042, 116.4074);
                LocationStatus = "Location permission not granted";
                LocationUpdated?.Invoke();
                return;
            }

            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            _currentLocation = await Geolocation.GetLocationAsync(request);
            LocationStatus = _currentLocation != null ? "Location success" : "Location not available";
        }
        catch (FeatureNotSupportedException)
        {
            _currentLocation = new Location(39.9042, 116.4074);
            LocationStatus = "Device not support location";
        }
        catch (FeatureNotEnabledException)
        {
            _currentLocation = new Location(39.9042, 116.4074);
            LocationStatus = "Location not enabled";
        }
        catch (PermissionException)
        {
            _currentLocation = new Location(39.9042, 116.4074);
            LocationStatus = "Location permission denied";
        }
        catch (Exception ex)
        {
            _currentLocation = new Location(39.9042, 116.4074);
            LocationStatus = "Location fail";
            System.Diagnostics.Debug.WriteLine($"[GeolocationService] Error: {ex.Message}");
        }

        LocationUpdated?.Invoke();
    }

    public double GetCompassHeading()
    {
        try { return _compassService.CurrentHeading; }
        catch { return 0; }
    }

    public string GetDirectionTo(double targetLat, double targetLon)
    {
        if (_currentLocation == null) return "Unknown direction";
        var bearing = CalculateBearing(_currentLocation.Latitude, _currentLocation.Longitude, targetLat, targetLon);
        return BearingToDirection(bearing);
    }

    private static double CalculateBearing(double lat1, double lon1, double lat2, double lon2)
    {
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var y = Math.Sin(dLon) * Math.Cos(lat2 * Math.PI / 180);
        var x = Math.Cos(lat1 * Math.PI / 180) * Math.Sin(lat2 * Math.PI / 180) -
                Math.Sin(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) * Math.Cos(dLon);
        return (Math.Atan2(y, x) * 180 / Math.PI + 360) % 360;
    }

    private static string BearingToDirection(double bearing) => bearing switch
    {
        >= 337.5 or < 22.5 => "North",
        >= 22.5 and < 67.5 => "Northeast",
        >= 67.5 and < 112.5 => "East",
        >= 112.5 and < 157.5 => "Southeast",
        >= 157.5 and < 202.5 => "South",
        >= 202.5 and < 247.5 => "Southwest",
        >= 247.5 and < 292.5 => "West",
        >= 292.5 and < 337.5 => "Northwest",
        _ => "Unknown"
    };

    public List<StoreInfo> GetNearbyStores()
    {
        var stores = new List<StoreInfo>
        {
            new() { Name = "Yonghui Supermarket Chaoyang Branch", Address = "No.88 Jianguo Road, Chaoyang District", Distance = 0.5, Type = "Supermarket", Latitude = 39.9087, Longitude = 116.4605 },
            new() { Name = "Hema Fresh Wangjing Store", Address = "No.10 Wangjing West Road, Chaoyang District", Distance = 0.8, Type = "Supermarket", Latitude = 39.9942, Longitude = 116.4701 },
            new() { Name = "FamilyMart Sanlitun Store", Address = "No.19 Sanlitun Road, Chaoyang District", Distance = 0.3, Type = "Convenience", Latitude = 39.9345, Longitude = 116.4539 },
            new() { Name = "7-Eleven Guomao Store", Address = "No.1 Jianguomenwai Avenue, Chaoyang District", Distance = 1.2, Type = "Convenience", Latitude = 39.9085, Longitude = 116.4594 },
            new() { Name = "Chaoyang District Fresh Market", Address = "No.12 Tuanjiehu Road, Chaoyang District", Distance = 0.6, Type = "Market", Latitude = 39.9289, Longitude = 116.4612 },
            new() { Name = "Sanyuanli Fresh Market", Address = "No.16 Sanyuanli Street, Chaoyang District", Distance = 1.5, Type = "Market", Latitude = 39.9513, Longitude = 116.4620 },
            new() { Name = "Pagoda Fruit Chaoyang Joy City Store", Address = "No.101 Chaoyang North Road, Chaoyang District", Distance = 0.9, Type = "Fruit Shop", Latitude = 39.9234, Longitude = 116.4743 },
            new() { Name = "Lawson Jianwai Store", Address = "No.6A Jianwai Avenue, Chaoyang District", Distance = 0.4, Type = "Convenience", Latitude = 39.9078, Longitude = 116.4551 },
            new() { Name = "Hualian Supermarket Jinsong Branch", Address = "No.1 Jinsong South Road, Chaoyang District", Distance = 2.1, Type = "Supermarket", Latitude = 39.8752, Longitude = 116.4636 },
            new() { Name = "Xianfeng Fruit Shuangjing Store", Address = "No.36 Guangqu Road, Chaoyang District", Distance = 1.0, Type = "Fruit Shop", Latitude = 39.8963, Longitude = 116.4628 },
        };

        // If user location available, calculate actual distance using MAUI built-in method
        if (_currentLocation != null)
        {
            foreach (var store in stores)
            {
                var storeLocation = new Location(store.Latitude, store.Longitude);
                store.Distance = Math.Round(
                    Location.CalculateDistance(_currentLocation, storeLocation, DistanceUnits.Kilometers), 1);
            }
        }

        return stores.OrderBy(s => s.Distance).ToList();
    }
}
