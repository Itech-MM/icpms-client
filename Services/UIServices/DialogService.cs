using System.Windows;
using icpms_client.Common.UI;
using icpms_client.Network.Request.Shift;
using icpms_client.Utils.UI.Dialog;
using icpms_client.Utils.UI.Dialog.ExportDialog;
using icpms_client.Utils.UI.Dialog.MessageDialog;
using icpms_client.Utils.UI.Dialog.Shift;

namespace icpms_client.Services.UIServices;

public abstract class DialogService
{
    private static LoadingDialog? _loadingDialog;
    private static StartShiftDialog? _startShiftDialog;
    
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

    public static void ShowStartShiftDialog(Action<StartShiftRequest?> callback, string? title = "Start Shift")
    {
        if (_startShiftDialog != null) return;

        var owner = BaseWindow.MainWindowInstance;
        
        _startShiftDialog = new StartShiftDialog
        {
            Owner = owner,
            ShiftStarted = callback,
            Title = title??"Start Shift"
        };
        
        owner.IsEnabled = false;
        owner.ApplyBlur(true);
        _startShiftDialog.Show();
    }
    
    public static void HideStartShiftDialog()
    {
        _startShiftDialog?.Close();
        var owner = BaseWindow.MainWindowInstance;
        owner.ApplyBlur(false);
        if (_startShiftDialog?.Owner != null)
        {
            _startShiftDialog.Owner.IsEnabled = true;
        }
        _startShiftDialog = null;
    }
    
}