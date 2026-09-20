using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using icpms_client.Common.Command;
using icpms_client.Network.DTO.Gate;
using icpms_client.Network.Response;
using icpms_client.Network.Services.Gate;
using icpms_client.Services.DeviceDiagnosis;
using icpms_client.Services.UIServices;
using log4net;

namespace icpms_client.Pages.Screens.ViewModels;

public class DeviceDiagnosisViewModel : ObservableBase
{
    private const int MaxParallelChecks = 6;
    private const string AllKey = "All";

    private readonly ILog _log = LogManager.GetLogger(typeof(DeviceDiagnosisViewModel));
    private readonly GateService _gateService;
    private readonly IDeviceDiagnosisService _diagnosisService;

    private CancellationTokenSource? _runCts;
    private int _loadSeq;
    private bool _isLoading;
    private bool _isRunning;
    private string? _errorMessage;
    private string _searchText = string.Empty;
    private string _progressText = string.Empty;
    private string _statusFilterKey = AllKey;

    public DeviceDiagnosisViewModel(
        GateService gateService,
        IDeviceDiagnosisService diagnosisService)
    {
        _gateService = gateService;
        _diagnosisService = diagnosisService;

        SummaryTiles =
        [
            new(AllKey, "ALL DEVICES"),
            new("Online", "ONLINE"),
            new("Degraded", "DEGRADED"),
            new("Offline", "OFFLINE"),
            new("Unknown", "NOT CHECKED")
        ];
        SummaryTiles[0].IsSelected = true;

        DevicesView = CollectionViewSource.GetDefaultView(Devices);
        DevicesView.Filter = FilterDevice;

        RefreshCommand = new RelayCommand(async _ => await LoadAsync());
        CheckAllCommand = new RelayCommand(async _ => await CheckAllAsync());
        CancelCommand = new RelayCommand(_ => CancelRun());
        SetStatusFilterCommand = new RelayCommand(p => SetStatusFilter(p as string));
    }

    public ObservableCollection<DeviceDiagnosisItemViewModel> Devices { get; } = new();

    public ICollectionView DevicesView { get; }

    public ObservableCollection<DeviceSummaryTile> SummaryTiles { get; }

    public ICommand RefreshCommand { get; }

    public ICommand CheckAllCommand { get; }

    public ICommand CancelCommand { get; }

    public ICommand SetStatusFilterCommand { get; }

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetField(ref _isLoading, value))
                OnPropertyChanged(nameof(ShowEmptyState));
        }
    }

    public bool IsRunning
    {
        get => _isRunning;
        private set => SetField(ref _isRunning, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (!SetField(ref _errorMessage, value))
                return;

            OnPropertyChanged(nameof(HasError));
            OnPropertyChanged(nameof(ShowEmptyState));
        }
    }

    public bool HasError => !string.IsNullOrEmpty(_errorMessage);

    public bool ShowEmptyState => !_isLoading && !HasError && Devices.Count == 0;

    public string ProgressText
    {
        get => _progressText;
        private set => SetField(ref _progressText, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetField(ref _searchText, value))
                DevicesView.Refresh();
        }
    }

    public async Task LoadAsync()
    {
        var seq = ++_loadSeq;
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await _gateService.GetGateDevices();
            if (seq != _loadSeq)
                return;

            if (TryExtractDevices(response, out var devices, out var error))
                MergeDevices(devices);
            else
                ErrorMessage = error;
        }
        catch (Exception ex)
        {
            _log.Error($"LoadAsync Error: {ex.Message}");
            if (seq == _loadSeq)
                ErrorMessage = "Failed to load devices.";
        }
        finally
        {
            if (seq == _loadSeq)
                IsLoading = false;

            RefreshSummary();
        }
    }

    public void CancelRun() => _runCts?.Cancel();

    private static bool TryExtractDevices(object? response, out List<GateDeviceDto> devices, out string error)
    {
        devices = new List<GateDeviceDto>();
        error = string.Empty;

        switch (response)
        {
            case BaseResponse<List<GateDeviceDto>> success:
                devices = success.Data ?? new List<GateDeviceDto>();
                return true;
            case BaseErrorResponse<string> failure:
                error = string.IsNullOrWhiteSpace(failure.Message) ? "Failed to load devices." : failure.Message;
                return false;
            default:
                error = "Failed to load devices.";
                return false;
        }
    }

    private void MergeDevices(List<GateDeviceDto> incoming)
    {
        var existing = Devices.ToDictionary(d => d.Id);
        var incomingIds = new HashSet<long>();

        foreach (var dto in incoming)
        {
            var id = Convert.ToInt64(dto.Id);
            incomingIds.Add(id);

            if (existing.TryGetValue(id, out var item))
                item.UpdateDevice(dto);
            else
                Devices.Add(new DeviceDiagnosisItemViewModel(dto, CheckAndSyncSingleAsync));
        }

        for (var i = Devices.Count - 1; i >= 0; i--)
        {
            if (!incomingIds.Contains(Devices[i].Id))
                Devices.RemoveAt(i);
        }
    }

    private async Task CheckAllAsync()
    {
        if (IsRunning || Devices.Count == 0)
            return;

        var targets = Devices.ToList();
        var completed = 0;
        var runCts = new CancellationTokenSource();
        _runCts = runCts;
        IsRunning = true;
        ProgressText = $"CHECKING 0 / {targets.Count}";

        var finished = new List<DeviceDiagnosisItemViewModel>();
        var finishedLock = new object();

        try
        {
            using var throttle = new SemaphoreSlim(MaxParallelChecks);
            await Task.WhenAll(targets.Select(async item =>
            {
                await throttle.WaitAsync(runCts.Token);
                try
                {
                    var ok = await CheckItemAsync(item, runCts.Token);
                    if (ok)
                    {
                        lock (finishedLock)
                            finished.Add(item);
                    }
                }
                finally
                {
                    throttle.Release();
                    var done = Interlocked.Increment(ref completed);
                    ProgressText = $"CHECKING {done} / {targets.Count}";
                }
            }));
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _runCts = null;
            runCts.Dispose();
            IsRunning = false;
            ProgressText = string.Empty;
            RefreshSummary();
        }

        List<DeviceDiagnosisItemViewModel> toSync;
        lock (finishedLock)
            toSync = finished.ToList();

        if (toSync.Count > 0)
            await SyncBatchAsync(toSync);
    }

    private async Task CheckAndSyncSingleAsync(DeviceDiagnosisItemViewModel item)
    {
        var ok = await CheckItemAsync(item, CancellationToken.None);
        if (ok)
            await SyncBatchAsync(new[] { item });
    }

    private async Task<bool> CheckItemAsync(DeviceDiagnosisItemViewModel item, CancellationToken token)
    {
        if (item.IsChecking)
            return false;

        item.IsChecking = true;
        try
        {
            item.ApplyResult(await _diagnosisService.DiagnoseAsync(item.Device, token));
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        catch (Exception ex)
        {
            _log.Error($"Diagnosis failed for '{item.Name}': {ex.Message}");
            item.ApplyResult(DeviceDiagnosisResult.Offline($"Diagnosis error: {ex.Message}"));
            return true;
        }
        finally
        {
            item.IsChecking = false;
            RefreshSummary();
        }
    }

    private async Task SyncBatchAsync(IReadOnlyCollection<DeviceDiagnosisItemViewModel> items)
    {
        if (items.Count == 0)
            return;

        var request = new GateDeviceBatchStatusRequestDto
        {
            Devices = items.Select(i => new GateDeviceStatusUpdateDto
            {
                DeviceId = i.Id,
                HealthStatus = (int)i.HealthState,
                LatencyMs = i.LastResult?.LatencyMs,
                StatusNote = i.LastResult?.Note ?? string.Empty,
                DeviceResponse = string.Empty
            }).ToList()
        };

        foreach (var item in items)
            item.MarkSyncing();

        try
        {
            var response = await _gateService.UpdateBatchStatusAsync(request);

            switch (response)
            {
                case BaseResponse<GateDeviceBatchStatusResponseDto> ok:
                {
                    var data = ok.Data ?? new GateDeviceBatchStatusResponseDto();

                    var failedMap = data.Failed?
                        .GroupBy(f => f.DeviceId)
                        .ToDictionary(g => g.Key, g => g.First().Reason)
                        ?? new Dictionary<long, string>();

                    foreach (var item in items)
                    {
                        if (failedMap.TryGetValue(item.Id, out var reason))
                            item.MarkSyncFailed(string.IsNullOrWhiteSpace(reason)
                                ? "Sync rejected by server."
                                : reason);
                        else
                            item.MarkSynced();
                    }

                    if (failedMap.Count > 0)
                    {
                        ToastService.ShowWarning(
                            $"{failedMap.Count} of {items.Count} device(s) failed to sync.",
                            "Partial sync");
                    }

                    break;
                }

                case BaseErrorResponse<string> err:
                {
                    var msg = string.IsNullOrWhiteSpace(err.Message) ? "Sync failed." : err.Message;

                    foreach (var item in items)
                        item.MarkSyncFailed(msg);

                    ToastService.ShowError(msg, "Sync failed");
                    break;
                }

                default:
                {
                    const string msg = "Unexpected sync response from server.";

                    foreach (var item in items)
                        item.MarkSyncFailed(msg);

                    ToastService.ShowError(msg, "Sync failed");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _log.Error($"SyncBatchAsync Error: {ex.Message}", ex);

            foreach (var item in items)
                item.MarkSyncFailed(ex.Message);

            ToastService.ShowError(ex.Message, "Sync failed");
        }
    }

    private void SetStatusFilter(string? key)
    {
        var next = string.IsNullOrEmpty(key) || key == _statusFilterKey ? AllKey : key;
        _statusFilterKey = next;

        foreach (var tile in SummaryTiles)
            tile.IsSelected = tile.Key == next;

        DevicesView.Refresh();
    }

    private bool FilterDevice(object obj)
    {
        if (obj is not DeviceDiagnosisItemViewModel item)
            return false;

        if (_statusFilterKey != AllKey && item.HealthState.ToString() != _statusFilterKey)
            return false;

        var term = _searchText.Trim();
        return term.Length == 0 || item.Matches(term);
    }

    private void RefreshSummary()
    {
        var counts = Devices.GroupBy(d => d.HealthState).ToDictionary(g => g.Key, g => g.Count());

        foreach (var tile in SummaryTiles)
            tile.Count = CountFor(tile.Key, counts);

        OnPropertyChanged(nameof(ShowEmptyState));

        if (_statusFilterKey != AllKey)
            DevicesView.Refresh();
    }

    private int CountFor(string key, IReadOnlyDictionary<DeviceHealthState, int> counts)
    {
        if (key == AllKey)
            return Devices.Count;

        return Enum.TryParse<DeviceHealthState>(key, out var state) && counts.TryGetValue(state, out var count)
            ? count
            : 0;
    }
}