using System.ComponentModel;
using icpms_client.Common.Command;
using icpms_client.Network.DTO.AuditLogs;

namespace icpms_client.Pages.Screens.ViewModels;

public enum AlertSeverity
{
    Danger,
    Warning,
    Neutral
}

public class VehicleAlertItemViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public long Id { get; }
    public string PlateNumber { get; }
    public string Message { get; }
    public string TagLabel { get; }
    public string PrimaryActionLabel { get; }
    public AlertSeverity Severity { get; }

    private bool _isDismissing;
    public bool IsDismissing
    {
        get => _isDismissing;
        set
        {
            _isDismissing = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDismissing)));
        }
    }

    public RelayCommand PrimaryActionCommand { get; }
    public RelayCommand DismissCommand { get; }

    public VehicleAlertItemViewModel(VehicleAlertDto dto, Func<VehicleAlertItemViewModel, Task> onDismiss,
        Action<VehicleAlertItemViewModel> onPrimaryAction)
    {
        Id = dto.Id;
        PlateNumber = dto.PlateNumber ?? "-";
        Message = dto.Message ?? string.Empty;

        var desc = dto.AlertTypeDesc?.ToLowerInvariant() ?? string.Empty;

        if (desc.Contains("blacklist"))
        {
            Severity = AlertSeverity.Danger;
            TagLabel = "Flagged";
            PrimaryActionLabel = "Inspect";
        }
        else if (desc.Contains("expired"))
        {
            Severity = AlertSeverity.Warning;
            TagLabel = "Overstay";
            PrimaryActionLabel = "Dispatch Guard";
        }
        else
        {
            Severity = AlertSeverity.Neutral;
            TagLabel = "Unknown";
            PrimaryActionLabel = "Inspect";
        }

        DismissCommand = new RelayCommand(async _ =>
        {
            if (IsDismissing) return;
            IsDismissing = true;
            try { await onDismiss(this); }
            finally { IsDismissing = false; }
        }, _ => !IsDismissing);

        PrimaryActionCommand = new RelayCommand(_ => onPrimaryAction(this));
    }
}