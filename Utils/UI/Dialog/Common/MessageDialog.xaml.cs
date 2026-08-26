using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FontAwesome.WPF;

namespace icpms_client.Utils.UI.Dialog.Common;

// MessageBoxDialog.xaml.cs
public partial class MessageBoxDialog : BaseDialog, INotifyPropertyChanged
{
    private MessageBoxResult _result;
    private MessageBoxButton _buttons;

    public MessageBoxResult Result => _result;
    
    public bool ShowCancel => _buttons == MessageBoxButton.OKCancel;
    public bool ShowYesNo => _buttons == MessageBoxButton.YesNo || _buttons == MessageBoxButton.YesNoCancel;

    public FontAwesomeIcon Icon { get; set; }
    public string Message { get; set; }

    public MessageBoxDialog(string message, string title, MessageBoxButton buttons, MessageBoxImage icon)
    {
        Message = message;
        Title = title;
        _buttons = buttons;
        SetIcon(icon);
        
        DataContext = this;
        InitializeComponent();
    }

    private void SetIcon(MessageBoxImage icon)
    {
        Icon = icon switch
        {
            MessageBoxImage.Error => FontAwesomeIcon.TimesCircle,
            MessageBoxImage.Question => FontAwesomeIcon.Question,
            MessageBoxImage.Information => FontAwesomeIcon.Info,
            MessageBoxImage.Warning => FontAwesomeIcon.Warning,
            _ => FontAwesomeIcon.None
        };
        OnPropertyChanged(nameof(Icon));
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        _result = Enum.Parse<MessageBoxResult>((string)((Button)sender).Tag);
        Close();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}