namespace icpms_client.Common.Threads;

public interface IPortWatcher
{
    void OnPortChanged(string[]? portNames);
}
public class SimplePortWatcher : IPortWatcher
{
    private readonly Action<string[]?> _onPortChangedHandler;

    public SimplePortWatcher(Action<string[]?> onPortChangedHandler)
    {
        _onPortChangedHandler = onPortChangedHandler;
    }

    public void OnPortChanged(string[]? portNames)
    {
        _onPortChangedHandler?.Invoke(portNames);
    }
}