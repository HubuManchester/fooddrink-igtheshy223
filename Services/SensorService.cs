namespace ssk.Services;

public class SensorService
{
    private bool _isMonitoring;
    private DateTime _lastShakeTime = DateTime.MinValue;

    public event Action? ShakeDetected;

    public void StartMonitoring()
    {
        if (_isMonitoring) return;
        _isMonitoring = true;
        try
        {
            Accelerometer.ShakeDetected += OnShakeDetected;
            Accelerometer.Start(SensorSpeed.Game);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Sensor error: {ex.Message}");
            _isMonitoring = false;
        }
    }

    public void StopMonitoring()
    {
        if (!_isMonitoring) return;
        try
        {
            Accelerometer.ShakeDetected -= OnShakeDetected;
            Accelerometer.Stop();
        }
        catch { }
        _isMonitoring = false;
    }

    private void OnShakeDetected(object? sender, EventArgs e)
    {
        if ((DateTime.Now - _lastShakeTime).TotalMilliseconds < 1000) return;
        _lastShakeTime = DateTime.Now;
        ShakeDetected?.Invoke();
    }
}
