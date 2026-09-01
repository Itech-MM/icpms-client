using System.Windows;
using icpms_client.Common.UI;
using icpms_client.Utils.UI.Toast;

namespace icpms_client.Services.UIServices;

public abstract class ToastService
{
    private static ToastHost? _host;

    public static void ShowSuccess(string message, string? title = null, TimeSpan? duration = null)
        => Show(ToastType.Success, message, title, duration ?? TimeSpan.FromSeconds(15));

    public static void ShowError(string message, string? title = null, TimeSpan? duration = null)
        => Show(ToastType.Error, message, title, duration ?? TimeSpan.FromSeconds(15));

    public static void ShowWarning(string message, string? title = null, TimeSpan? duration = null)
        => Show(ToastType.Warning, message, title, duration ?? TimeSpan.FromSeconds(15));

    public static void ShowInfo(string message, string? title = null, TimeSpan? duration = null)
        => Show(ToastType.Info, message, title, duration ?? TimeSpan.FromSeconds(15));

    public static void Show(ToastType type, string message, string? title = null, TimeSpan? duration = null)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null) return;

        if (!dispatcher.CheckAccess())
        {
            dispatcher.Invoke(() => Show(type, message, title, duration));
            return;
        }

        EnsureHost();
        _host!.AddToast(type, message, title, duration ?? TimeSpan.FromSeconds(4));
    }

    private static void EnsureHost()
    {
        if (_host != null) return;
        _host = new ToastHost(BaseWindow.MainWindowInstance);
    }
}