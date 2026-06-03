namespace ssk.Services;

public class SpeechRecognitionService
{
#if ANDROID
    private Android.Speech.SpeechRecognizer? _speechRecognizer;
    private TaskCompletionSource<string?>? _tcs;

    public async Task<string?> RecognizeAsync(string language = "zh-CN")
    {
        try
        {
            var status = await Permissions.RequestAsync<Permissions.Microphone>();
            if (status != PermissionStatus.Granted)
            {
                await Shell.Current.DisplayAlert("Permission Not Enough", "Need microphone permission for voice input", "OK");
                return null;
            }

            var activity = Platform.CurrentActivity;
            if (activity == null) return null;

            if (!Android.Speech.SpeechRecognizer.IsRecognitionAvailable(activity))
            {
                await Shell.Current.DisplayAlert("Not Support", "Current device not support speech recognition", "OK");
                return null;
            }

            _tcs = new TaskCompletionSource<string?>();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                _speechRecognizer = Android.Speech.SpeechRecognizer.CreateSpeechRecognizer(activity)!;
                _speechRecognizer.SetRecognitionListener(new SpeechListener(this));
                var intent = new Android.Content.Intent(Android.Speech.RecognizerIntent.ActionRecognizeSpeech);
                intent.PutExtra(Android.Speech.RecognizerIntent.ExtraLanguageModel,
                    Android.Speech.RecognizerIntent.LanguageModelFreeForm);
                intent.PutExtra(Android.Speech.RecognizerIntent.ExtraLanguage, language);
                intent.PutExtra(Android.Speech.RecognizerIntent.ExtraMaxResults, 1);
                _speechRecognizer.StartListening(intent);
            });

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            cts.Token.Register(() => _tcs.TrySetResult(null));

            var result = await _tcs.Task;
            Cleanup();
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SpeechRecognition] Error: {ex.Message}");
            Cleanup();
            return null;
        }
    }

    private void Cleanup()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                _speechRecognizer?.StopListening();
                _speechRecognizer?.Destroy();
                _speechRecognizer = null;
            }
            catch { }
        });
    }

    private void OnResult(string? text) => _tcs?.TrySetResult(text);
    private void OnError() => _tcs?.TrySetResult(null);

    private class SpeechListener : Java.Lang.Object, Android.Speech.IRecognitionListener
    {
        private readonly SpeechRecognitionService _service;
        public SpeechListener(SpeechRecognitionService service) => _service = service;

        public void OnResults(Android.OS.Bundle? results)
        {
            if (results == null) { _service.OnResult(null); return; }
            var matches = results.GetStringArrayList(Android.Speech.SpeechRecognizer.ResultsRecognition);
            _service.OnResult(matches?.Count > 0 ? matches[0] : null);
        }

        public void OnError(Android.Speech.SpeechRecognizerError error)
        {
            System.Diagnostics.Debug.WriteLine($"[SpeechRecognition] Error: {error}");
            _service.OnError();
        }

        public void OnBeginningOfSpeech() { }
        public void OnBufferReceived(byte[]? buffer) { }
        public void OnEndOfSpeech() { }
        public void OnEvent(int eventType, Android.OS.Bundle? @params) { }
        public void OnPartialResults(Android.OS.Bundle? partialResults) { }
        public void OnReadyForSpeech(Android.OS.Bundle? @params) { }
        public void OnRmsChanged(float rmsdB) { }
    }
#else
    public async Task<string?> RecognizeAsync(string language = "zh-CN")
    {
        await Shell.Current.DisplayAlert("Tips", "Voice input only available on Android device", "OK");
        return null;
    }
#endif
}
