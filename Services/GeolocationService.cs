using ssk.Models;

namespace ssk.Services;

public class GeolocationService
{
    private readonly CompassService _compassService;
    private Location? _currentLocation;

    public Location? CurrentLocation => _currentLocation;
    public string LocationStatus { get; private set; } = "正在获取位置...";

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
                // 无权限时使用模拟位置（北京）
                _currentLocation = new Location(39.9042, 116.4074);
                LocationStatus = "使用模拟位置（无定位权限）";
                LocationUpdated?.Invoke();
                return;
            }

            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            _currentLocation = await Geolocation.GetLocationAsync(request);
            LocationStatus = _currentLocation != null ? "定位成功" : "使用模拟位置";
        }
        catch (FeatureNotSupportedException)
        {
            _currentLocation = new Location(39.9042, 116.4074);
            LocationStatus = "设备不支持定位，使用模拟位置";
        }
        catch (FeatureNotEnabledException)
        {
            _currentLocation = new Location(39.9042, 116.4074);
            LocationStatus = "定位未开启，使用模拟位置";
        }
        catch (PermissionException)
        {
            _currentLocation = new Location(39.9042, 116.4074);
            LocationStatus = "使用模拟位置（无定位权限）";
        }
        catch (Exception ex)
        {
            _currentLocation = new Location(39.9042, 116.4074);
            LocationStatus = $"定位失败，使用模拟位置";
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
        if (_currentLocation == null) return "未知方向";
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
        >= 337.5 or < 22.5 => "正北",
        >= 22.5 and < 67.5 => "东北",
        >= 67.5 and < 112.5 => "正东",
        >= 112.5 and < 157.5 => "东南",
        >= 157.5 and < 202.5 => "正南",
        >= 202.5 and < 247.5 => "西南",
        >= 247.5 and < 292.5 => "正西",
        >= 292.5 and < 337.5 => "西北",
        _ => "未知"
    };

    public List<StoreInfo> GetNearbyStores()
    {
        var stores = new List<StoreInfo>
        {
            new() { Name = "永辉超市（朝阳店）", Address = "朝阳区建国路88号", Distance = 0.5, Type = "超市", Latitude = 39.9087, Longitude = 116.4605 },
            new() { Name = "盒马鲜生（望京店）", Address = "朝阳区望京西路10号", Distance = 0.8, Type = "超市", Latitude = 39.9942, Longitude = 116.4701 },
            new() { Name = "全家便利店（三里屯店）", Address = "朝阳区三里屯路19号", Distance = 0.3, Type = "便利", Latitude = 39.9345, Longitude = 116.4539 },
            new() { Name = "7-11便利店（国贸店）", Address = "朝阳区建国门外大街1号", Distance = 1.2, Type = "便利", Latitude = 39.9085, Longitude = 116.4594 },
            new() { Name = "朝阳区菜市场", Address = "朝阳区团结湖路12号", Distance = 0.6, Type = "菜场", Latitude = 39.9289, Longitude = 116.4612 },
            new() { Name = "三源里菜市场", Address = "朝阳区三源里街16号", Distance = 1.5, Type = "菜场", Latitude = 39.9513, Longitude = 116.4620 },
            new() { Name = "百果园（朝阳大悦城店）", Address = "朝阳区朝阳北路101号", Distance = 0.9, Type = "水果", Latitude = 39.9234, Longitude = 116.4743 },
            new() { Name = "罗森便利店（建外店）", Address = "朝阳区建外大街甲6号", Distance = 0.4, Type = "便利", Latitude = 39.9078, Longitude = 116.4551 },
            new() { Name = "华联超市（劲松店）", Address = "朝阳区劲松南路1号", Distance = 2.1, Type = "超市", Latitude = 39.8752, Longitude = 116.4636 },
            new() { Name = "鲜丰水果（双井店）", Address = "朝阳区广渠路36号", Distance = 1.0, Type = "水果", Latitude = 39.8963, Longitude = 116.4628 },
        };

        // 如果有用户位置，用 MAUI 内置方法计算实际距离
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
