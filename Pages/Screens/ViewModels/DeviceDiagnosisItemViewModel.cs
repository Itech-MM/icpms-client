using System.Windows.Input;
using icpms_client.Common.Command;
using icpms_client.Network.DTO.Gate;
using icpms_client.Services.DeviceDiagnosis;

namespace icpms_client.Pages.Screens.ViewModels;

public enum DeviceSyncState
{
    NotSynced,
    Syncing,
    Synced,
    Failed
}

public class DeviceDiagnosisItemViewModel : ObservableBase
{
    private readonly Func<DeviceDiagnosisItemViewModel, Task> _checkAsync;

    private GateDeviceDto _device = null!;
    private DeviceHealthState _healthState;
    private int? _latencyMs;
    private DateTime? _checkedAt;
    private string _note = string.Empty;
    private bool _isChecking;
    private bool _isSimulated;
    private bool _hasLocalResult;

    private DeviceSyncState _syncState = DeviceSyncState.NotSynced;
    private string? _syncErrorMessage;
    private DeviceDiagnosisResult? _lastResult;

    public DeviceDiagnosisItemViewModel(GateDeviceDto device, Func<DeviceDiagnosisItemViewModel, Task> checkAsync)
    {
        _checkAsync = checkAsync;
        CheckCommand = new RelayCommand(async _ => await _checkAsync(this));
        UpdateDevice(device);
    }

    public ICommand CheckCommand { get; }

    public GateDeviceDto Device => _device;

    public long Id { get; private set; }

    public string Name => _device.Name;

    public string GateName => string.IsNullOrWhiteSpace(_device.GateName) ? "—" : _device.GateName;

    public GateDeviceKind Kind => DeviceKinds.ToKind(_device.DeviceType);

    public string KindLabel => string.IsNullOrWhiteSpace(_device.DeviceTypeDesc) ? Kind.ToString() : _device.DeviceTypeDesc;

    public DeviceConnectionKind ConnectionKind => DeviceKinds.ToConnection(_device.ConnectionType);

    public string ConnectionLabel => ConnectionKind == DeviceConnectionKind.Serial ? "SERIAL" : "LAN";

    public string Endpoint
    {
        get
        {
            if (ConnectionKind == DeviceConnectionKind.Serial)
            {
                var com = string.IsNullOrWhiteSpace(_device.ComPort) ? "—" : _device.ComPort;
                return _device.BaudRate is > 0 ? $"{com} @ {_device.BaudRate}" : com;
            }

            var ip = string.IsNullOrWhiteSpace(_device.IpAddress) ? "—" : _device.IpAddress;
            return _device.Port is > 0 ? $"{ip}:{_device.Port}" : ip;
        }
    }

    public DeviceHealthState HealthState => _healthState;

    public string HealthLabel => _healthState switch
    {
        DeviceHealthState.Online => "ONLINE",
        DeviceHealthState.Degraded => "DEGRADED",
        DeviceHealthState.Offline => "OFFLINE",
        _ => "NOT CHECKED"
    };

    public string LatencyText => _latencyMs.HasValue ? $"{_latencyMs.Value} ms" : "—";

    public string CheckedAtText => _checkedAt.HasValue ? _checkedAt.Value.ToString("dd MMM HH:mm:ss") : "Never";

    public string Note => string.IsNullOrWhiteSpace(_note) ? "—" : _note;

    public bool IsSimulated => _isSimulated;

    public bool IsChecking
    {
        get => _isChecking;
        set => SetField(ref _isChecking, value);
    }

    public DeviceSyncState SyncState
    {
        get => _syncState;
        private set => SetField(ref _syncState, value);
    }

    public string? SyncErrorMessage
    {
        get => _syncErrorMessage;
        private set => SetField(ref _syncErrorMessage, value);
    }

    public DeviceDiagnosisResult? LastResult => _lastResult;

    public void UpdateDevice(GateDeviceDto device)
    {
        _device = device;
        Id = Convert.ToInt64(device.Id);

        if (!_hasLocalResult)
            ApplyBackendStatus();

        OnPropertyChanged(string.Empty);
    }

    public void ApplyResult(DeviceDiagnosisResult result)
    {
        _hasLocalResult = true;
        _healthState = result.State;
        _latencyMs = result.LatencyMs;
        _checkedAt = result.CheckedAt;
        _note = result.Note;
        _isSimulated = result.Simulated;
        _lastResult = result;

        _syncState = DeviceSyncState.NotSynced;
        _syncErrorMessage = null;

        OnPropertyChanged(string.Empty);
    }

    public void MarkSyncing()
    {
        SyncState = DeviceSyncState.Syncing;
        SyncErrorMessage = null;
    }

    public void MarkSynced()
    {
        SyncState = DeviceSyncState.Synced;
        SyncErrorMessage = null;
    }

    public void MarkSyncFailed(string reason)
    {
        SyncState = DeviceSyncState.Failed;
        SyncErrorMessage = reason;
    }

    public bool Matches(string term) =>
        Contains(Name, term)
        || Contains(GateName, term)
        || Contains(KindLabel, term)
        || Contains(_device.IpAddress, term)
        || Contains(_device.ComPort, term)
        || Contains(_note, term);

    private void ApplyBackendStatus()
    {
        _healthState = DeviceKinds.ToHealth(_device.LastHealthStatus);
        _latencyMs = _device.LastLatencyMs;
        _checkedAt = _device.LastCheckedAt;
        _note = _device.LastStatusNote ?? string.Empty;
        _isSimulated = false;
    }

    private static bool Contains(string? source, string term) =>
        source != null && source.Contains(term, StringComparison.OrdinalIgnoreCase);
}