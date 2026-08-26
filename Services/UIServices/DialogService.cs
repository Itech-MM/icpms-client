using System.Windows;
using icpms_client.Common.UI;
using icpms_client.Utils.UI.Dialog;
using icpms_client.Utils.UI.Dialog.ExportDialog;
using icpms_client.Utils.UI.Dialog.MessageDialog;

namespace icpms_client.Services.UIServices;

public abstract class DialogService
{
    private static LoadingDialog? _loadingDialog;
    
    public static void ShowLoadingDialog(BaseWindow? owner , string? message = null)
    {
        if (_loadingDialog != null) return;
        owner ??= BaseWindow.MainWindowInstance;
        _loadingDialog = new LoadingDialog
        {
            Owner = owner,
            LoadingText = message??"Loading..."
        };
        owner.IsEnabled = false;
        owner.ApplyBlur(true);
        _loadingDialog.Show();
    }
    
    
    public static void HideLoading(BaseWindow owner)
    {
        _loadingDialog?.Close();
        owner.ApplyBlur(false);
        if (_loadingDialog?.Owner != null)
        {
            _loadingDialog.Owner.IsEnabled = true;
        }
        _loadingDialog = null;
    }

    public static void ShowAuthDialog(BaseWindow owner, string? message = null)
    {
        ShowErrorDialog(message);
    }

    public static MessageBoxResult ShowErrorDialog(string? message = null, BaseWindow? owner = null)
    {
        owner ??= BaseWindow.MainWindowInstance;
        owner.ApplyBlur(true);
        return StyleMessageBox.Show(
            owner: owner,
            message: message ?? "An error occurred",
            caption: "Error!",
            buttons: MessageBoxButton.OK,
            icon: MessageBoxImage.Error
        );
    }
    
    public static MessageBoxResult ShowWarningDialog(string? message = null, MessageBoxButton button = MessageBoxButton.OK, string? title = "Warning",BaseWindow? owner = null)
    {
        owner ??= BaseWindow.MainWindowInstance;
        return StyleMessageBox.Show(
            owner: owner,
            message: message ?? "Warning!",
            caption: title?? "Warning!",
            buttons: button,
            icon: MessageBoxImage.Warning
        );
    }

    
    public static MessageBoxResult ShowQuestionDialog(string? message = null, string? title = null, BaseWindow? owner = null)
    {
        owner ??= BaseWindow.MainWindowInstance;
        return StyleMessageBox.Show(
            owner: owner,
            message: message ?? "An error occurred",
            caption: title ?? "Warning!",
            buttons: MessageBoxButton.YesNoCancel,
            icon: MessageBoxImage.Question
        );
    }
    
    public static MessageBoxResult ShowSuccessDialog(string? message = null)
    {
        return StyleMessageBox.Show(
            BaseWindow.MainWindowInstance, 
            message??"Success!", "Success!", 
            MessageBoxButton.OK, 
            MessageBoxImage.Information);
    }

    public static void ShowExportDialog(Action<ExportType> callback)
    {
        var owner = BaseWindow.MainWindowInstance;
        var dialog = new ExportDialog
        {
            Owner = owner
        };
        owner.ApplyBlur(true);
        dialog.OnExport += (t) =>
        {
            owner.ApplyBlur(false);
            callback(t);
        };
        dialog.ShowDialog();
    }
}