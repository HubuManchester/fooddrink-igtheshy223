using System.Windows.Input;
using ssk.Services;

namespace ssk.Controls;

public partial class VoiceEditor : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(VoiceEditor), defaultBindingMode: BindingMode.TwoWay);
    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(VoiceEditor), "");
    public static readonly BindableProperty FontSizeProperty =
        BindableProperty.Create(nameof(FontSize), typeof(double), typeof(VoiceEditor), 14.0);
    public static readonly BindableProperty EditorHeightProperty =
        BindableProperty.Create(nameof(EditorHeight), typeof(double), typeof(VoiceEditor), 60.0);
    public static readonly BindableProperty IsListeningProperty =
        BindableProperty.Create(nameof(IsListening), typeof(bool), typeof(VoiceEditor), false);

    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public string Placeholder { get => (string)GetValue(PlaceholderProperty); set => SetValue(PlaceholderProperty, value); }
    public double FontSize { get => (double)GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }
    public double EditorHeight { get => (double)GetValue(EditorHeightProperty); set => SetValue(EditorHeightProperty, value); }
    public bool IsListening { get => (bool)GetValue(IsListeningProperty); set => SetValue(IsListeningProperty, value); }

    public ICommand VoiceCommand { get; }

    public VoiceEditor()
    {
        InitializeComponent();
        VoiceCommand = new Command(async () => await ExecuteVoiceInput());
    }

    private async Task ExecuteVoiceInput()
    {
        var service = Application.Current?.Handler?.MauiContext?.Services.GetService<SpeechRecognitionService>();
        if (service == null) return;

        IsListening = true;
        try
        {
            var result = await service.RecognizeAsync();
            if (!string.IsNullOrEmpty(result))
                Text = string.IsNullOrEmpty(Text) ? result : Text + "\n" + result;
        }
        finally
        {
            IsListening = false;
        }
    }
}
