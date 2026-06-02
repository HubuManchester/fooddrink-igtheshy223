using System.Windows.Input;
using ssk.Services;

namespace ssk.Controls;

public partial class VoiceEntry : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(VoiceEntry), defaultBindingMode: BindingMode.TwoWay);
    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(VoiceEntry), "");
    public static readonly BindableProperty KeyboardProperty =
        BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(VoiceEntry), Keyboard.Default);
    public static readonly BindableProperty FontSizeProperty =
        BindableProperty.Create(nameof(FontSize), typeof(double), typeof(VoiceEntry), 14.0);
    public static readonly BindableProperty IsListeningProperty =
        BindableProperty.Create(nameof(IsListening), typeof(bool), typeof(VoiceEntry), false);

    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public string Placeholder { get => (string)GetValue(PlaceholderProperty); set => SetValue(PlaceholderProperty, value); }
    public Keyboard Keyboard { get => (Keyboard)GetValue(KeyboardProperty); set => SetValue(KeyboardProperty, value); }
    public double FontSize { get => (double)GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }
    public bool IsListening { get => (bool)GetValue(IsListeningProperty); set => SetValue(IsListeningProperty, value); }

    public ICommand VoiceCommand { get; }

    public VoiceEntry()
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
                Text = result;
        }
        finally
        {
            IsListening = false;
        }
    }
}
