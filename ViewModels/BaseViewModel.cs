using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ssk.ViewModels;

public class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        MainThread.BeginInvokeOnMainThread(() =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
    }

    public ICommand CreateCommand(Action execute, Func<bool>? canExecute = null)
        => new Command(execute, canExecute ?? (() => !IsBusy));

    public ICommand CreateCommand<T>(Action<T> execute, Func<T, bool>? canExecute = null)
        => new Command<T>(execute, canExecute ?? (_ => !IsBusy));

    public ICommand CreateAsyncCommand(Func<Task> execute, Func<bool>? canExecute = null)
        => new Command(async () => { IsBusy = true; try { await execute(); } finally { IsBusy = false; } }, canExecute ?? (() => !IsBusy));

    public ICommand CreateAsyncCommand<T>(Func<T, Task> execute, Func<T, bool>? canExecute = null)
        => new Command<T>(async (p) => { IsBusy = true; try { await execute(p); } finally { IsBusy = false; } }, canExecute ?? (_ => !IsBusy));
}
