namespace ssk.Services;

public class CompassService
{
    private bool _isMonitoring;

    public double CurrentHeading { get; private set; }
    public event Action<double>? HeadingChanged;

    public void Start()
    {
        if (_isMonitoring) return;
        try
        {
            Compass.ReadingChanged += OnReadingChanged;
            Compass.Start(SensorSpeed.UI);
            _isMonitoring = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Compass error: {ex.Message}");
        }
    }

    public void Stop()
    {
        if (!_isMonitoring) return;
        try
        {
            Compass.ReadingChanged -= OnReadingChanged;
            Compass.Stop();
        }
        catch { }
        _isMonitoring = false;
    }

    private void OnReadingChanged(object? sender, CompassChangedEventArgs e)
    {
        CurrentHeading = e.Reading.HeadingMagneticNorth;
        HeadingChanged?.Invoke(CurrentHeading);
    }
}
