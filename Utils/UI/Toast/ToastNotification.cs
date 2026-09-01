using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using icpms_client.Common.Command;

namespace icpms_client.Utils.UI.Toast;

public class ToastNotification : INotifyPropertyChanged
{
    public Guid Id { get; } = Guid.NewGuid();
    public ToastType Type { get; }
    public string Message { get; }
    public string? Title { get; }
    public bool HasTitle => !string.IsNullOrWhiteSpace(Title);

    public RelayCommand DismissCommand { get; }

    public event Action<ToastNotification>? RequestClose;

    private readonly DispatcherTimer? _timer;

    public TimeSpan Duration { get; }
    public bool ShowProgress => Duration > TimeSpan.Zero;

    public ToastNotification(ToastType type, string message, string? title, TimeSpan duration)
    {
        Type = type;
        Message = message;
        Title = title;
        Duration = duration;

        DismissCommand = new RelayCommand(_ => RequestClose?.Invoke(this));

        if (duration > TimeSpan.Zero)
        {
            _timer = new DispatcherTimer { Interval = duration };
            _timer.Tick += (_, _) =>
            {
                _timer!.Stop();
                RequestClose?.Invoke(this);
            };
            _timer.Start();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}