using System.Collections.ObjectModel;
using System.Windows.Input;
using ssk.Models;
using ssk.Services;

namespace ssk.ViewModels;

public class NearbyStoresViewModel : BaseViewModel
{
    private readonly GeolocationService _geolocationService;

    private string _locationCoordinates = "Getting location...";
    public string LocationCoordinates
    {
        get => _locationCoordinates;
        set => SetProperty(ref _locationCoordinates, value);
    }

    private string _locationStatus = "Getting location...";
    public string LocationStatus
    {
        get => _locationStatus;
        set => SetProperty(ref _locationStatus, value);
    }

    public ObservableCollection<StoreInfo> Stores { get; } = new();

    private string _storeCountText = string.Empty;
    public string StoreCountText
    {
        get => _storeCountText;
        set => SetProperty(ref _storeCountText, value);
    }

    private double _compassHeading;
    public double CompassHeading
    {
        get => _compassHeading;
        set => SetProperty(ref _compassHeading, value);
    }

    public double Latitude => _geolocationService.CurrentLocation?.Latitude ?? 0;
    public double Longitude => _geolocationService.CurrentLocation?.Longitude ?? 0;

    private string _compassDirection = string.Empty;
    public string CompassDirection
    {
        get => _compassDirection;
        set => SetProperty(ref _compassDirection, value);
    }

    public ICommand RefreshLocationCommand { get; }
    public ICommand OpenStoreCommand { get; }

    public NearbyStoresViewModel(GeolocationService geolocationService)
    {
        _geolocationService = geolocationService;
        Title = "Nearby Stores";

        RefreshLocationCommand = CreateAsyncCommand(ExecuteRefreshLocation);
        OpenStoreCommand = CreateAsyncCommand<StoreInfo>(ExecuteOpenStore);

        _geolocationService.LocationUpdated += OnLocationUpdated;
    }

    public async Task InitializeAsync()
    {
        await ExecuteRefreshLocation();
    }

    private async Task ExecuteRefreshLocation()
    {
        try
        {
            await _geolocationService.RefreshLocationAsync();
            UpdateLocationDisplay();
            LoadStores();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NearbyStoresViewModel] RefreshLocation error: {ex.Message}");
            LocationStatus = "Location fail";
        }
    }

    private void UpdateLocationDisplay()
    {
        LocationStatus = _geolocationService.LocationStatus;

        if (_geolocationService.CurrentLocation != null)
        {
            var loc = _geolocationService.CurrentLocation;
            LocationCoordinates = $"Latitude {loc.Latitude:F6} Longitude {loc.Longitude:F6}";
        }
        else
        {
            LocationCoordinates = "Cannot get location info";
        }

        CompassHeading = _geolocationService.GetCompassHeading();
        OnPropertyChanged(nameof(Latitude));
        OnPropertyChanged(nameof(Longitude));
    }

    private void LoadStores()
    {
        try
        {
            var stores = _geolocationService.GetNearbyStores();

            // re-sort distance by current location if have location info
            if (_geolocationService.CurrentLocation != null)
            {
                var loc = _geolocationService.CurrentLocation;
                foreach (var store in stores)
                {
                    store.Distance = CalculateDistance(loc.Latitude, loc.Longitude, store.Latitude, store.Longitude);
                }
                stores = stores.OrderBy(s => s.Distance).ToList();
            }

            Stores.Clear();
            foreach (var store in stores)
                Stores.Add(store);

            StoreCountText = $"Nearby total {stores.Count} stores";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NearbyStoresViewModel] LoadStores error: {ex.Message}");
        }
    }

    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return Math.Round(6371 * c, 1); // earth radius km
    }

    private async Task ExecuteOpenStore(StoreInfo store)
    {
        try
        {
            var location = new Location(store.Latitude, store.Longitude);
            var options = new MapLaunchOptions { Name = store.Name };
            await Map.OpenAsync(location, options);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NearbyStoresViewModel] OpenStore error: {ex.Message}");
            await Shell.Current.DisplayAlert("Hint", "Cannot open map application", "OK");
        }
    }

    private void OnLocationUpdated()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            UpdateLocationDisplay();
            LoadStores();
        });
    }
}
