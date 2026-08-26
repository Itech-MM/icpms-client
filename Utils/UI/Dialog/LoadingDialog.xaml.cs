using System.ComponentModel;
using System.Windows;

namespace icpms_client.Utils.UI.Dialog;

public partial class LoadingDialog : Window, INotifyPropertyChanged
{
    private string _loadingText;
    
    public LoadingDialog()
    {
        _loadingText = "Loading, please wait...";
        InitializeComponent();
    }

    public string LoadingText
    {
        get => _loadingText;
        set
        {
            _loadingText = value;
            OnPropertyChanged(nameof(LoadingText));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    
    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}