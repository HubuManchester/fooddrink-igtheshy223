using System.Collections.ObjectModel;
using ssk.Models;

namespace ssk.Services;

public class TextToSpeechService
{
    private CancellationTokenSource? _cts;
    private bool _isSpeaking;

    public bool IsSpeaking => _isSpeaking;

    public event Action<bool>? SpeakingStateChanged;

    public async Task SpeakAsync(string text)
    {
        Stop();
        _cts = new CancellationTokenSource();
        _isSpeaking = true;
        SpeakingStateChanged?.Invoke(true);
        try
        {
            var settings = new SpeechOptions { Pitch = 1.0f, Volume = 1.0f };
            await TextToSpeech.SpeakAsync(text, settings, _cts.Token);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"TTS Error: {ex.Message}"); }
        finally
        {
            _isSpeaking = false;
            SpeakingStateChanged?.Invoke(false);
        }
    }

    public async Task SpeakStepsAsync(ObservableCollection<StepModel> steps)
    {
        Stop();
        _cts = new CancellationTokenSource();
        _isSpeaking = true;
        SpeakingStateChanged?.Invoke(true);
        try
        {
            foreach (var step in steps)
            {
                if (_cts.Token.IsCancellationRequested) break;
                var text = $"{step.DisplayIndex}, {step.Text}";
                var settings = new SpeechOptions { Pitch = 1.0f, Volume = 1.0f };
                await TextToSpeech.SpeakAsync(text, settings, _cts.Token);
                if (!_cts.Token.IsCancellationRequested && step != steps.Last())
                    await Task.Delay(500, _cts.Token);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"TTS Steps Error: {ex.Message}"); }
        finally
        {
            _isSpeaking = false;
            SpeakingStateChanged?.Invoke(false);
        }
    }

    public void Stop()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        _isSpeaking = false;
        SpeakingStateChanged?.Invoke(false);
    }
}
