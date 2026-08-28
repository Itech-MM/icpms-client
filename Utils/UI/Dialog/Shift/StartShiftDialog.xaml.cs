using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using icpms_client.Network.Request.Shift;
using log4net;
namespace icpms_client.Utils.UI.Dialog.Shift;

public partial class StartShiftDialog : Window, INotifyPropertyChanged
{
    private readonly ILog _log = LogManager.GetLogger(typeof(StartShiftDialog));

    private string? _openingCashText;
    private string? _remark;
    private string _errorMessage = string.Empty;

    private string _title = "Start Shift";

    // Invoked with the entered values on confirm, or null on cancel.
    public Action<StartShiftRequest?>? ShiftStarted;

    private Dispatcher _uiDispatcher;

    public StartShiftDialog()
    {
        InitializeComponent();
        DataContext = this;
        _uiDispatcher = Dispatcher.CurrentDispatcher;
    }

    public string? OpeningCashText
    {
        get => _openingCashText;
        set
        {
            _openingCashText = value;
            OnPropertyChanged();
        }
    }

    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            OnPropertyChanged();
        }
    }

    public string? Remark
    {
        get => _remark;
        set
        {
            _remark = value;
            OnPropertyChanged();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    private void OnCancelClicked(object sender, RoutedEventArgs e)
    {
        ShiftStarted?.Invoke(null);
    }

    private void OnConfirmClick(object sender, RoutedEventArgs e)
    {
        try
        {
            decimal? openingCash = null;

            if (!string.IsNullOrWhiteSpace(OpeningCashText))
            {
                if (!decimal.TryParse(OpeningCashText, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
                {
                    ErrorMessage = "Opening cash must be a valid number.";
                    return;
                }

                if (parsed < 0)
                {
                    ErrorMessage = "Opening cash cannot be negative.";
                    return;
                }

                openingCash = parsed;
            }

            var result = new StartShiftRequest
            {
                OpeningCash = openingCash,
                Remark = string.IsNullOrWhiteSpace(Remark) ? null : Remark,
            };

            _log.Info($"Shift started with opening cash: {result.OpeningCash?.ToString(CultureInfo.InvariantCulture) ?? "N/A"}, remark: {result.Remark ?? "N/A"} at {DateTime.Now}");
            ErrorMessage = string.Empty;
            ShiftStarted?.Invoke(result);
        }
        catch (Exception ex)
        {
            _log.Error(ex);
            ErrorMessage = ex.Message;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}