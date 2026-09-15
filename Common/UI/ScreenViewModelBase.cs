using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace icpms_client.Common.UI;

public abstract class ScreenViewModelBase : IDisposableScreen, IAsyncDisposable, IDisposable, INotifyPropertyChanged
{
    private readonly List<IDisposable> _children = [];
    private readonly object _disposeLock = new();
    private bool _disposed;

    protected void RegisterChild(IDisposable child)
    {
        _children.Add(child);
    }

    public virtual void OnNavigatedFrom() { }

    public void Dispose()
    {
        if (!TryBeginDispose())
            return;

        DisposeChildren();
        OnDispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (!TryBeginDispose())
            return;

        DisposeChildren();
        OnDispose();
        await OnDisposeAsync().ConfigureAwait(false);
    }

    private bool TryBeginDispose()
    {
        lock (_disposeLock)
        {
            if (_disposed)
                return false;

            _disposed = true;
            return true;
        }
    }

    private void DisposeChildren()
    {
        foreach (var child in _children)
            child.Dispose();

        _children.Clear();
    }

    protected virtual void OnDispose() { }

    protected virtual Task OnDisposeAsync() => Task.CompletedTask;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}