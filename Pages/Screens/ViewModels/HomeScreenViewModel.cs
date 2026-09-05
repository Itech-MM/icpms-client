using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using icpms_client.Common.Command;
using icpms_client.Common.Constants;
using icpms_client.Common.UI;
using icpms_client.Network.DTO;
using icpms_client.Network.DTO.AuditLogs;
using icpms_client.Network.DTO.Common;
using icpms_client.Network.DTO.Gate;
using icpms_client.Network.Request.Visitor;
using icpms_client.Network.Response;
using icpms_client.Network.Response.Home;
using icpms_client.Network.Response.Vehicle;
using icpms_client.Network.Response.Visitor;
using icpms_client.Network.Services.Home;
using icpms_client.Network.Services.Logs;
using icpms_client.Network.Services.Realtime;
using icpms_client.Network.Services.Vehicle;
using icpms_client.Network.Services.Visitor;
using icpms_client.Services.ExternalServices;
using icpms_client.Services.UIServices;
using icpms_client.Tools.VideoPlayer;
using log4net;
using VehicleDetection.Contracts;

namespace icpms_client.Pages.Screens.ViewModels;

public class HomeScreenViewModel : INotifyPropertyChanged
{
    private readonly ILog _log = LogManager.GetLogger(nameof(HomeScreenViewModel));

    public Action<bool>? OnVisitorSaved;
    public Action<bool>? OnExitVisitorSaved;
    
    private readonly HomeScreenService _homeScreenService;

    private readonly VehicleService _vehicleService;

    private readonly VisitorService _visitorService;

    private bool _isPreloadLoading = true;
    private string? _loadError;


    private bool _exitGateOpen;
    private string? _exitGateStateText = "Closed";
    private string? _exitStatusText;

    private bool _exitPlateMatched;
    private string? _exitParkedDurationText;
    private string? _exitFeeDueText;
    private bool _isChoosingPaymentMethod;
    private bool _isReadyToConfirmPayment;
    private bool _isPaymentComplete;

    private RtspPlayer? _entranceAnprPlayer;
    private RtspPlayer? _entranceCctvPlayer;
    private RtspPlayer? _exitAnprPlayer;
    private RtspPlayer? _exitCctvPlayer;

    private string? _entranceAnprUrl;
    private string? _entranceCctvUrl;
    private string? _exitAnprUrl;
    private string? _exitCctvUrl;

    private string _entranceAnprName = "CAM 01";
    private string _entranceCctvName = "CAM 02";
    private string _exitAnprName = "CAM 03";
    private string _exitCctvName = "CAM 04";

    private string _entranceDetectedVehicleNo = "------";
    private bool _entranceIsMember;
    private bool _entranceIsVip;

    private string _exitDetectedVehicleNo = "------";
    private bool _exitIsMember;
    private bool _exitIsVip;

    private ImageSource? _entranceSnapshotImage;
    private ImageSource? _exitSnapshotImage;

    private bool _isSnapshotModalOpen;
    private ImageSource? _modalSnapshotImage;
    private string? _modalSnapshotTitle;

    private bool _entranceHasSnapshot;
    private bool _exitHasSnapshot;

    private SnapshotSide _activeSnapshotSide;

    private int _entranceRequestSeq;
    private int _exitRequestSeq;

    private bool _entranceManualEntryRequired;
    private string? _entranceManualEntryReason;
    
    private ObservableCollection<CommonObject> _paymentMethods = new();
    private CommonObject? _selectedPaymentMethod;
    private bool _isFocChecked;
    private bool _isPaymentModalOpen;
    private bool _isShowPayments;
    
    private string? _exitRemark;


    private readonly VehicleAlertRealtimeService _alertService;
    private readonly VehicleAlertLogService _vehicleAlertLogService;
    
    private int _currentPage = 1;
    private bool _hasNextPage = true;

    private bool _isLoadingMoreAlerts;

    public ObservableCollection<VehicleAlertItemViewModel> SecurityAlerts { get; } = new();

    public bool HasActiveAlerts => SecurityAlerts.Count > 0;
    public string ActiveAlertCountText => $"{SecurityAlerts.Count} Active";

    
    public HomeScreenViewModel(HomeScreenService homeScreenService, VehicleDetectionRealtimeService vehicleDetectionRealtimeService,
        VehicleService vehicleService, VisitorService visitorService, VehicleAlertRealtimeService alertService,
        VehicleAlertLogService vehicleAlertLogService)
    {
        _homeScreenService = homeScreenService;

        vehicleDetectionRealtimeService.VehicleDetected += OnVehicleDetected;

        _vehicleService = vehicleService;

        _visitorService = visitorService;
        
        _alertService = alertService;
        _alertService.AlertReceived += OnAlertReceived;
        
        _vehicleAlertLogService = vehicleAlertLogService;
        

        

        OpenExitGateCommand = new RelayCommand(async void (_) => await OpenExitGateAsync(), _ => ExitPlateMatched);
        ConfirmPaymentCommand = new RelayCommand(async void (_) => await ConfirmPaymentAsync(), _ => ExitPlateMatched);
        SelectCardPaymentCommand = new RelayCommand(_ => SelectPaymentMethod("CARD"), _ => ExitPlateMatched);
        SelectCashPaymentCommand = new RelayCommand(_ => SelectPaymentMethod("CASH"), _ => ExitPlateMatched);
        RetryLoadCommand = new RelayCommand(async void (_) => await InitializeAsync());

        RefreshEntranceCameraCommand = new RelayCommand(_ => RefreshEntranceCamera());
        RefreshExitCameraCommand = new RelayCommand(_ => RefreshExitCamera());

        TestEntranceEntryCommand = new RelayCommand(_ => TestEntranceEntry());
        TestExitEntryCommand = new RelayCommand(_ => TestExitEntry());
        OpenEntranceSnapshotCommand = new RelayCommand(_ => OpenSnapshotModal(SnapshotSide.Entrance), _ => EntranceHasSnapshot);
        OpenExitSnapshotCommand = new RelayCommand(_ => OpenSnapshotModal(SnapshotSide.Exit), _ => ExitHasSnapshot);
        CloseSnapshotModalCommand = new RelayCommand(_ => CloseSnapshotModal());
        RecaptureSnapshotCommand = new RelayCommand(_ => RecaptureActiveSnapshot());

        RetryEntranceVisitorSaveCommand = new RelayCommand(async void (_) => await RetryEntranceVisitorSaveAsync(), _ => EntranceManualEntryRequired);
        DismissEntranceManualEntryCommand = new RelayCommand(_ => DismissEntranceManualEntry(), _ => EntranceManualEntryRequired);
        
        OpenPaymentModalCommand = new RelayCommand(_ => OpenPaymentModal(), _ => ExitPlateMatched);
        ClosePaymentModalCommand = new RelayCommand(_ => IsPaymentModalOpen = false);
        ConfirmPaymentModalCommand = new RelayCommand(
            async void (_) => await ConfirmPaymentAsync(),
            _ => ExitIsMember || IsFocChecked || SelectedPaymentMethod != null);
    }
    
    private async Task LoadSecurityAlertsAsync(int pageNo = 1, bool initialize = false)
    {
        try
        {
            if (initialize)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    SecurityAlerts.Clear();
                    RaiseAlertCountChanged();
                });

                _currentPage = 1;
                _hasNextPage = true;
                pageNo = 1;
            }
            
            var response = await _vehicleAlertLogService.SearchLogs(pageNo);

            if (response is BaseResponse<SearchResultDto<VehicleAlertDto>> { Success: true } success)
            {
                var page = success.Data;
                var results = page?.Results ?? new List<VehicleAlertDto>();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var dto in results)
                        SecurityAlerts.Add(new VehicleAlertItemViewModel(dto, OnAlertDismissedAsync, OnAlertPrimaryAction));

                    RaiseAlertCountChanged();
                });

                _currentPage = page?.PageNo ?? pageNo;
                _hasNextPage = page?.HasNextPage ?? false;
            }
            else if (response is BaseErrorResponse<string> error)
            {
                _log.Error($"Failed to load vehicle alerts: {error.Message}");
            }
        }
        catch (Exception ex)
        {
            _log.Error($"LoadSecurityAlertsAsync error: {ex.Message}");
        }
    }

    public async Task LoadMoreSecurityAlertsAsync()
    {
        if (IsLoadingMoreAlerts || !_hasNextPage) return;

        IsLoadingMoreAlerts = true;
        try
        {
            await LoadSecurityAlertsAsync(_currentPage + 1);
        }
        finally
        {
            IsLoadingMoreAlerts = false;
        }
    }
    
    private void OnAlertReceived(object? sender, VehicleAlertDto dto)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var item = new VehicleAlertItemViewModel(dto, OnAlertDismissedAsync, OnAlertPrimaryAction);
            SecurityAlerts.Insert(0, item);
            RaiseAlertCountChanged();
        });
    }
    
    private async Task OnAlertDismissedAsync(VehicleAlertItemViewModel item)
    {
        var response = await _vehicleAlertLogService.UpdateStatus(item.Id);

        if (response is BaseErrorResponse<string> error)
        {
            _log.Error($"Dismiss failed for alert {item.Id}: {error.Message}");
            return;
        }

        Application.Current.Dispatcher.Invoke(() =>
        {
            SecurityAlerts.Remove(item);
            RaiseAlertCountChanged();
        });
    }

    private void OnAlertPrimaryAction(VehicleAlertItemViewModel item)
    {
        // TODO: hook up to whatever "inspect" / "dispatch guard" actually does —
        // e.g. open the snapshot modal, or notify security via another API call
    }
    
    

    private void RaiseAlertCountChanged()
    {
        OnPropertyChanged(nameof(HasActiveAlerts));
        OnPropertyChanged(nameof(ActiveAlertCountText));
    }

    public bool IsPreloadLoading
    {
        get => _isPreloadLoading;
        private set { _isPreloadLoading = value; OnPropertyChanged(); }
    }

    public string? LoadError
    {
        get => _loadError;
        private set { _loadError = value; OnPropertyChanged(); }
    }


    public bool ExitGateOpen
    {
        get => _exitGateOpen;
        set { _exitGateOpen = value; ExitGateStateText = value ? "Open" : "Closed"; OnPropertyChanged(); }
    }

    public string? ExitGateStateText
    {
        get => _exitGateStateText;
        private set { _exitGateStateText = value; OnPropertyChanged(); }
    }

    public string? ExitStatusText
    {
        get => _exitStatusText;
        set { _exitStatusText = value; OnPropertyChanged(); }
    }
    
    public bool IsLoadingMoreAlerts
    {
        get => _isLoadingMoreAlerts;
        set
        {
            _isLoadingMoreAlerts = value;
            OnPropertyChanged();
        }
    }

    public bool ExitPlateMatched
    {
        get => _exitPlateMatched;
        set
        {
            _exitPlateMatched = value;
            OnPropertyChanged();

            OpenExitGateCommand.RaiseCanExecuteChanged();
            ConfirmPaymentCommand.RaiseCanExecuteChanged();
            SelectCardPaymentCommand.RaiseCanExecuteChanged();
            SelectCashPaymentCommand.RaiseCanExecuteChanged();
        }
    }

    public string? ExitParkedDurationText
    {
        get => _exitParkedDurationText;
        set { _exitParkedDurationText = value; OnPropertyChanged(); }
    }

    public string? ExitFeeDueText
    {
        get => _exitFeeDueText;
        set { _exitFeeDueText = value; OnPropertyChanged(); }
    }

    public bool IsChoosingPaymentMethod
    {
        get => _isChoosingPaymentMethod;
        set { _isChoosingPaymentMethod = value; OnPropertyChanged(); }
    }

    public bool IsReadyToConfirmPayment
    {
        get => _isReadyToConfirmPayment;
        set { _isReadyToConfirmPayment = value; OnPropertyChanged(); }
    }

    public bool IsPaymentComplete
    {
        get => _isPaymentComplete;
        set { _isPaymentComplete = value; OnPropertyChanged(); }
    }

    public string EntranceDetectedVehicleNo
    {
        get => _entranceDetectedVehicleNo;
        set { _entranceDetectedVehicleNo = value; OnPropertyChanged(); }
    }

    public bool EntranceIsMember
    {
        get => _entranceIsMember;
        set { _entranceIsMember = value; OnPropertyChanged(); }
    }

    public bool EntranceIsVip
    {
        get => _entranceIsVip;
        set { _entranceIsVip = value; OnPropertyChanged(); }
    }

    public string ExitDetectedVehicleNo
    {
        get => _exitDetectedVehicleNo;
        set { _exitDetectedVehicleNo = value; OnPropertyChanged(); }
    }

    public bool ExitIsMember
    {
        get => _exitIsMember;
        set
        {
            _exitIsMember = value;
            OnPropertyChanged();
            ConfirmPaymentModalCommand.RaiseCanExecuteChanged();
        }
    }

    public bool ExitIsVip
    {
        get => _exitIsVip;
        set { _exitIsVip = value; OnPropertyChanged(); }
    }

    public string? ExitRemark
    {
        get => _exitRemark;
        set { _exitRemark = value; OnPropertyChanged(); }
    }
    
    public ObservableCollection<CommonObject> PaymentMethods
    {
        get => _paymentMethods;
        private set { _paymentMethods = value; OnPropertyChanged(); }
    }

    public CommonObject? SelectedPaymentMethod
    {
        get => _selectedPaymentMethod;
        set
        {
            _selectedPaymentMethod = value;
            OnPropertyChanged();
            ConfirmPaymentModalCommand.RaiseCanExecuteChanged();
        }
    }

    public bool IsFocChecked
    {
        get => _isFocChecked;
        set
        {
            _isFocChecked = value;
            OnPropertyChanged();
            ConfirmPaymentModalCommand.RaiseCanExecuteChanged();
        }
    }

    public bool IsPaymentModalOpen
    {
        get => _isPaymentModalOpen;
        private set { _isPaymentModalOpen = value; OnPropertyChanged(); }
    }

    public bool IsShowPayments
    {
        get => _isShowPayments;
        set
        {
            _isShowPayments = value;
            OnPropertyChanged();
        }
    }

    public ImageSource? EntranceSnapshotImage
    {
        get => _entranceSnapshotImage;
        private set { _entranceSnapshotImage = value; OnPropertyChanged(); }
    }

    public ImageSource? ExitSnapshotImage
    {
        get => _exitSnapshotImage;
        private set { _exitSnapshotImage = value; OnPropertyChanged(); }
    }

    public bool IsSnapshotModalOpen
    {
        get => _isSnapshotModalOpen;
        private set { _isSnapshotModalOpen = value; OnPropertyChanged(); }
    }

    public ImageSource? ModalSnapshotImage
    {
        get => _modalSnapshotImage;
        private set { _modalSnapshotImage = value; OnPropertyChanged(); }
    }

    public string? ModalSnapshotTitle
    {
        get => _modalSnapshotTitle;
        private set { _modalSnapshotTitle = value; OnPropertyChanged(); }
    }

    public bool EntranceHasSnapshot
    {
        get => _entranceHasSnapshot;
        private set
        {
            _entranceHasSnapshot = value;
            OnPropertyChanged();
            OpenEntranceSnapshotCommand.RaiseCanExecuteChanged();
        }
    }

    public bool ExitHasSnapshot
    {
        get => _exitHasSnapshot;
        private set
        {
            _exitHasSnapshot = value;
            OnPropertyChanged();
            OpenExitSnapshotCommand.RaiseCanExecuteChanged();
        }
    }

    public bool EntranceManualEntryRequired
    {
        get => _entranceManualEntryRequired;
        private set
        {
            _entranceManualEntryRequired = value;
            OnPropertyChanged();
            RetryEntranceVisitorSaveCommand.RaiseCanExecuteChanged();
            DismissEntranceManualEntryCommand.RaiseCanExecuteChanged();
        }
    }

    public string? EntranceManualEntryReason
    {
        get => _entranceManualEntryReason;
        private set { _entranceManualEntryReason = value; OnPropertyChanged(); }
    }

    public RtspPlayer? EntranceAnprPlayer
    {
        get => _entranceAnprPlayer;
        private set { _entranceAnprPlayer = value; OnPropertyChanged(); }
    }

    public RtspPlayer? EntranceCctvPlayer
    {
        get => _entranceCctvPlayer;
        private set { _entranceCctvPlayer = value; OnPropertyChanged(); }
    }

    public RtspPlayer? ExitAnprPlayer
    {
        get => _exitAnprPlayer;
        private set { _exitAnprPlayer = value; OnPropertyChanged(); }
    }

    public RtspPlayer? ExitCctvPlayer
    {
        get => _exitCctvPlayer;
        private set { _exitCctvPlayer = value; OnPropertyChanged(); }
    }

    public string EntranceAnprName
    {
        get => _entranceAnprName;
        set { _entranceAnprName = value; OnPropertyChanged(); }
    }

    public string EntranceCctvName
    {
        get => _entranceCctvName;
        set { _entranceCctvName = value; OnPropertyChanged(); }
    }

    public string ExitAnprName
    {
        get => _exitAnprName;
        set { _exitAnprName = value; OnPropertyChanged(); }
    }

    public string ExitCctvName
    {
        get => _exitCctvName;
        set { _exitCctvName = value; OnPropertyChanged(); }
    }

    public void InitializeEntranceAnprCamera(RtspPlayer player)
    {
        EntranceAnprPlayer = player;
        if (!string.IsNullOrWhiteSpace(_entranceAnprUrl)) player.Play(_entranceAnprUrl);
    }

    public void InitializeEntranceCctvCamera(RtspPlayer player)
    {
        EntranceCctvPlayer = player;
        if (!string.IsNullOrWhiteSpace(_entranceCctvUrl)) player.Play(_entranceCctvUrl);
    }

    public void InitializeExitAnprCamera(RtspPlayer player)
    {
        ExitAnprPlayer = player;
        if (!string.IsNullOrWhiteSpace(_exitAnprUrl)) player.Play(_exitAnprUrl);
    }

    public void InitializeExitCctvCamera(RtspPlayer player)
    {
        ExitCctvPlayer = player;
        if (!string.IsNullOrWhiteSpace(_exitCctvUrl)) player.Play(_exitCctvUrl);
    }

    public RelayCommand OpenExitGateCommand { get; }
    public RelayCommand ConfirmPaymentCommand { get; }
    public RelayCommand SelectCardPaymentCommand { get; }
    public RelayCommand SelectCashPaymentCommand { get; }
    public RelayCommand RetryLoadCommand { get; }

    public RelayCommand TestEntranceEntryCommand { get; }
    public RelayCommand TestExitEntryCommand { get; }

    public RelayCommand RefreshEntranceCameraCommand { get; }
    public RelayCommand RefreshExitCameraCommand { get; }

    public RelayCommand OpenEntranceSnapshotCommand { get; }
    public RelayCommand OpenExitSnapshotCommand { get; }
    public RelayCommand CloseSnapshotModalCommand { get; }
    public RelayCommand RecaptureSnapshotCommand { get; }

    public RelayCommand RetryEntranceVisitorSaveCommand { get; }
    public RelayCommand DismissEntranceManualEntryCommand { get; }
    
    public RelayCommand OpenPaymentModalCommand { get; }
    public RelayCommand ClosePaymentModalCommand { get; }
    public RelayCommand ConfirmPaymentModalCommand { get; }

    public async Task InitializeAsync()
    {
        LoadError = null;

        IsPreloadLoading = true;

        var preloadTask = LoadPreloadAsync();

        var loadAlertTask = LoadSecurityAlertsAsync(initialize: true);

        await Task.WhenAll(preloadTask, loadAlertTask);
    }

    private async Task LoadPreloadAsync()
    {
        try
        {
            var response = await _homeScreenService.GetHomeScreenPreload();

            if (response is BaseResponse<HomeScreenPreloadResponse> { Success: true, Data: not null } success)
            {
                ApplyPreload(success.Data);
            }
            else
            {
                LoadError = response?.Message ?? "Failed to load home screen data.";
            }
        }
        catch (Exception ex)
        {
            LoadError = ex.Message;
        }
        finally
        {
            IsPreloadLoading = false;
        }
    }

    private void ApplyPreload(HomeScreenPreloadResponse data)
    {
        var devices = data.Devices;

        _entranceAnprUrl = FindAccessUrl(devices, DeviceTypeCodes.Anpr, DirectionCodes.Entry);
        _entranceCctvUrl = FindAccessUrl(devices, DeviceTypeCodes.Cctv, DirectionCodes.Entry);
        _exitAnprUrl = FindAccessUrl(devices, DeviceTypeCodes.Anpr, DirectionCodes.Exit);
        _exitCctvUrl = FindAccessUrl(devices, DeviceTypeCodes.Cctv, DirectionCodes.Exit);

        EntranceAnprName = FindDeviceName(devices, DeviceTypeCodes.Anpr, DirectionCodes.Entry);
        EntranceCctvName = FindDeviceName(devices, DeviceTypeCodes.Cctv, DirectionCodes.Entry);
        ExitAnprName = FindDeviceName(devices, DeviceTypeCodes.Anpr, DirectionCodes.Exit);
        ExitCctvName = FindDeviceName(devices, DeviceTypeCodes.Cctv, DirectionCodes.Exit);

        PaymentMethods = new ObservableCollection<CommonObject>(data.PaymentMethods);

        
        if (!string.IsNullOrWhiteSpace(_entranceAnprUrl)) EntranceAnprPlayer?.Play(_entranceAnprUrl);
        if (!string.IsNullOrWhiteSpace(_entranceCctvUrl)) EntranceCctvPlayer?.Play(_entranceCctvUrl);
        if (!string.IsNullOrWhiteSpace(_exitAnprUrl)) ExitAnprPlayer?.Play(_exitAnprUrl);
        if (!string.IsNullOrWhiteSpace(_exitCctvUrl)) ExitCctvPlayer?.Play(_exitCctvUrl);
    }
    
    private void OpenPaymentModal()
    {
        SelectedPaymentMethod = null;
        IsFocChecked = false;
        ExitRemark = null;
        IsPaymentModalOpen = true;
    }

    private async Task ConfirmPaymentAsync()
    {
        var plateNumber = ExitDetectedVehicleNo;
        if (string.IsNullOrWhiteSpace(plateNumber) || plateNumber == "------") return;

        var owner = BaseWindow.MainWindowInstance;
        DialogService.ShowLoadingDialog(owner, "Processing payment...");

        Response? response;
        try
        {
            response = await _visitorService.SaveExitVisitor(new VisitorExitRequest
            {
                PlateNumber = plateNumber,
                PaymentMethod = SelectedPaymentMethod?.Code ?? 0,
                Remark = ExitRemark,
                IsMember = ExitIsMember,
                IsFoc = IsFocChecked
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exit visitor save failed for {plateNumber}: {ex.Message}");
            response = null;
        }
        finally
        {
            DialogService.HideLoading(owner);
        }

        if (response is not { Success: true })
        {
            var reason = response?.Message ?? "Failed to save visitor exit.";
            DialogService.ShowErrorDialog($"Payment failed for {plateNumber}: {reason}");
            return;
        }

        ToastService.ShowSuccess($"Successfully saved visitor exit for {plateNumber}");
        IsPaymentModalOpen = false;
        OnExitVisitorSaved?.Invoke(true);
        ResetExitData();
    }

    private static string? FindAccessUrl(IEnumerable<GateDeviceDto> devices, int deviceType, int direction)
    {
        var url = devices
            .FirstOrDefault(d => d.DeviceType == deviceType && d.Direction == direction)
            ?.AccessUrl;

        return string.IsNullOrWhiteSpace(url) ? null : url;
    }

    private static string FindDeviceName(IEnumerable<GateDeviceDto> devices, int deviceType, int direction)
    {
        var url = devices
            .FirstOrDefault(d => d.DeviceType == deviceType && d.Direction == direction)
            ?.Name;

        return string.IsNullOrWhiteSpace(url) ? "" : url;
    }

    private Task OpenExitGateAsync()
    {
        ExitGateOpen = true;
        return Task.CompletedTask;
    }

    private void SelectPaymentMethod(string method)
    {
        IsChoosingPaymentMethod = false;
        IsReadyToConfirmPayment = true;
    }

    private void RefreshEntranceCamera()
    {
        if (EntranceAnprPlayer != null && !string.IsNullOrWhiteSpace(_entranceAnprUrl)) _ = EntranceAnprPlayer.RefreshAsync(_entranceAnprUrl);
        if (EntranceCctvPlayer != null && !string.IsNullOrWhiteSpace(_entranceCctvUrl)) _ = EntranceCctvPlayer.RefreshAsync(_entranceCctvUrl);
    }

    private void RefreshExitCamera()
    {
        if (ExitAnprPlayer != null && !string.IsNullOrWhiteSpace(_exitAnprUrl))
            _ = ExitAnprPlayer.RefreshAsync(_exitAnprUrl);
        if (ExitCctvPlayer != null && !string.IsNullOrWhiteSpace(_exitCctvUrl))
            _ = ExitCctvPlayer.RefreshAsync(_exitCctvUrl);
    }

    private void TestEntranceEntry()
    {
        EntranceDetectedVehicleNo = "1A/1234";
        EntranceIsMember = true;
        EntranceIsVip = true;

        CaptureEntranceSnapshot();
    }

    private void TestExitEntry()
    {
        ExitDetectedVehicleNo = "2B/4321";
        ExitIsMember = true;
        ExitIsVip = false;

        ExitStatusText = "Plate matched";
        ExitParkedDurationText = "2h 15m";
        ExitFeeDueText = "5.00";

        ExitPlateMatched = true;
        IsChoosingPaymentMethod = true;
        IsReadyToConfirmPayment = false;
        IsPaymentComplete = false;

        CaptureExitSnapshot();
    }

    private static ImageSource? CaptureSnapshotImage(RtspPlayer? player)
    {
        if (player == null) return null;

        var tempFile = Path.Combine(Path.GetTempPath(), $"icpms_snapshot_{Guid.NewGuid():N}.png");
        var file = new FileInfo(tempFile);

        try
        {
            if (!player.Snapshot(file) || !File.Exists(tempFile)) return null;

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(tempFile);
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }
        catch
        {
            return null;
        }
        finally
        {
            try { if (File.Exists(tempFile)) File.Delete(tempFile); } catch { }
        }
    }

    private void CaptureEntranceSnapshot()
    {
        var captured = CaptureSnapshotImage(EntranceAnprPlayer);
        EntranceSnapshotImage = captured;
        EntranceHasSnapshot = captured != null;
    }

    private void CaptureExitSnapshot()
    {
        var captured = CaptureSnapshotImage(ExitAnprPlayer);
        ExitSnapshotImage = captured;
        ExitHasSnapshot = captured != null;
    }

    private void OpenSnapshotModal(SnapshotSide side)
    {
        _activeSnapshotSide = side;
        ModalSnapshotTitle = side == SnapshotSide.Entrance ? "Entrance Snapshot" : "Exit Snapshot";
        ModalSnapshotImage = side == SnapshotSide.Entrance ? EntranceSnapshotImage : ExitSnapshotImage;
        IsSnapshotModalOpen = true;
    }

    private void CloseSnapshotModal()
    {
        IsSnapshotModalOpen = false;
    }

    private void RecaptureActiveSnapshot()
    {
        if (_activeSnapshotSide == SnapshotSide.Entrance)
        {
            CaptureEntranceSnapshot();
            ModalSnapshotImage = EntranceSnapshotImage;
        }
        else
        {
            CaptureExitSnapshot();
            ModalSnapshotImage = ExitSnapshotImage;
        }
    }

    private void OnVehicleDetected(VehicleDetectedEvent evt)
    {
        switch (evt.DeviceIp.ToLowerInvariant())
        {
            case "entrance":
                _ = HandleEntranceDetectionAsync(evt);
                break;
            case "exit":
                _ = HandleExitDetectionAsync(evt);
                break;
            default:
                Console.WriteLine($"Vehicle event from unrecognized source '{evt.DeviceIp}' - check camera httpHosts config.");
                break;
        }
    }

    private async Task HandleEntranceDetectionAsync(VehicleDetectedEvent evt)
    {
        var mySeq = ++_entranceRequestSeq;

        Application.Current.Dispatcher.Invoke(() =>
        {
            ToastService.ShowInfo("Entrance vehicle detected.");
            EntranceDetectedVehicleNo = evt.LicensePlate;
            EntranceManualEntryRequired = false;
            EntranceManualEntryReason = null;
            CaptureEntranceSnapshot();
        });

        Response? response;
        try
        {
            response = await _vehicleService.GetVehicleDetailByPlateNumber(evt.LicensePlate);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Plate lookup failed for {evt.LicensePlate}: {ex.Message}");
            return;
        }

        if (mySeq != _entranceRequestSeq) return;

        if (response?.ErrorCode is "UNKNOWN_PLATE" or "VEHICLE_BLACKLISTED")
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                EntranceIsMember = false;
                EntranceIsVip = false;
                ToastService.ShowError(response.Message ?? "Unknown or blacklist vehicle detected.");
            });
            return;
        }

        Application.Current.Dispatcher.Invoke(() =>
        {
            if (response is BaseResponse<VehicleDetailResponse> { Success: true, Data: not null } success)
            {
                EntranceIsMember = success.Data.IsMember;
                EntranceIsVip = success.Data.IsVip;
            }
            else
            {
                EntranceIsMember = false;
                EntranceIsVip = false;
            }
        });

        await SaveEntranceVisitorAsync(mySeq, evt.LicensePlate, evt.PlateType);
    }

    private async Task SaveEntranceVisitorAsync(int mySeq, string plateNumber, string vehicleType)
    {
        Response? visitorResponse;
        try
        {
            visitorResponse = await _visitorService.SaveEntryVisitor(new VisitorEntryRequest
            {
                PlateNumber = plateNumber,
                VehicleType = vehicleType,
                ParkingSlotId = null
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Visitor entry save failed for {plateNumber}: {ex.Message}");
            visitorResponse = null;
        }

        if (mySeq != _entranceRequestSeq) return;

        if (visitorResponse is not { Success: true })
        {
            var reason = visitorResponse?.Message ?? "Failed to save visitor entry.";
            Console.WriteLine($"Visitor entry save unsuccessful for {plateNumber}: {reason}");

            Application.Current.Dispatcher.Invoke(() =>
            {
                EntranceManualEntryReason = reason;
                EntranceManualEntryRequired = true;
                DialogService.ShowErrorDialog($"Auto entry failed for {plateNumber}: {reason}\nPlease enter the vehicle manually.");
            });
        }
        else
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ToastService.ShowSuccess($"Successfully saved visitor entry for {plateNumber}");
                EntranceManualEntryReason = null;
                EntranceManualEntryRequired = false;
            });
            ResetEntranceData();
            OnVisitorSaved?.Invoke(true);
        }
    }

    private async Task RetryEntranceVisitorSaveAsync()
    {
        var mySeq = _entranceRequestSeq;
        var plateNumber = EntranceDetectedVehicleNo;

        if (string.IsNullOrWhiteSpace(plateNumber) || plateNumber == "------") return;

        await SaveEntranceVisitorAsync(mySeq, plateNumber, string.Empty);
    }

    private void DismissEntranceManualEntry()
    {
        EntranceManualEntryReason = null;
        EntranceManualEntryRequired = false;
    }

    private async Task HandleExitDetectionAsync(VehicleDetectedEvent evt)
{
    var mySeq = ++_exitRequestSeq;

    Application.Current.Dispatcher.Invoke(() =>
    {
        ExitDetectedVehicleNo = evt.LicensePlate;
        ExitParkedDurationText = null;
        ExitFeeDueText = null;
        CaptureExitSnapshot();
    });

    Response? response;
    try
    {
        response = await _vehicleService.GetVehicleDetailByPlateNumber(evt.LicensePlate);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Plate lookup failed for {evt.LicensePlate}: {ex.Message}");
        return;
    }

    if (mySeq != _exitRequestSeq) return;

    if (response?.ErrorCode is "UNKNOWN_PLATE" or "VEHICLE_BLACKLISTED")
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            ExitIsMember = false;
            ExitIsVip = false;
            ExitPlateMatched = false;
            IsShowPayments = true;
            ToastService.ShowError(response.Message ?? "Unknown or blacklist vehicle detected.");
        });
        return;
    }

    bool plateMatched = false;

    Application.Current.Dispatcher.Invoke(() =>
    {
        if (response is BaseResponse<VehicleDetailResponse> { Success: true, Data: not null } success)
        {
            ExitIsMember = success.Data.IsMember;
            ExitIsVip = success.Data.IsVip;
            ExitPlateMatched = success.Data.Vehicle != null;
            IsShowPayments = !ExitIsMember;
            plateMatched = ExitPlateMatched;
        }
        else
        {
            ExitIsMember = false;
            ExitIsVip = false;
            ExitPlateMatched = false;
            IsShowPayments = true;
        }
    });

    if (!plateMatched) return;

    Response? previewResponse;
    try
    {
        previewResponse = await _visitorService.GetExitPreview(evt.LicensePlate);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Exit preview failed for {evt.LicensePlate}: {ex.Message}");
        previewResponse = null;
    }

    if (mySeq != _exitRequestSeq) return;

    Application.Current.Dispatcher.Invoke(() =>
    {
        if (previewResponse is BaseResponse<ExitPreviewResponse> { Success: true, Data: not null } success)
        {
            ExitParkedDurationText = FormatDuration(success.Data.DurationMinutes);
            ExitFeeDueText = success.Data.AmountDue.ToString("0.00");
        }
        else
        {
            ExitParkedDurationText = null;
            ExitFeeDueText = null;
            ToastService.ShowError($"Could not load fee details for {evt.LicensePlate}.");
        }
    });
}

private static string FormatDuration(long totalMinutes)
{
    var hours = totalMinutes / 60;
    var minutes = totalMinutes % 60;
    return hours > 0 ? $"{hours}h {minutes}m" : $"{minutes}m";
}
    
    public void ResetEntranceData()
    {
        _entranceRequestSeq++;

        EntranceDetectedVehicleNo = "------";
        EntranceIsMember = false;
        EntranceIsVip = false;

        EntranceSnapshotImage = null;
        EntranceHasSnapshot = false;

        EntranceManualEntryRequired = false;
        EntranceManualEntryReason = null;
    }

    public void ResetExitData()
    {
        _exitRequestSeq++;

        ExitDetectedVehicleNo = "------";
        ExitIsMember = false;
        ExitIsVip = false;

        ExitSnapshotImage = null;
        ExitHasSnapshot = false;

        ExitGateOpen = false;
        ExitStatusText = null;
        ExitPlateMatched = false;
        ExitParkedDurationText = null;
        ExitFeeDueText = null;

        IsChoosingPaymentMethod = false;
        IsReadyToConfirmPayment = false;
        IsPaymentComplete = false;

        ExitRemark = null;
    }

    public void ResetAllData()
    {
        ResetEntranceData();
        ResetExitData();
    }

    private enum SnapshotSide
    {
        Entrance,
        Exit
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}