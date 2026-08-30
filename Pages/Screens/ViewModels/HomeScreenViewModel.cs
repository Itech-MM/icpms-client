using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using icpms_client.Common.Command;
using icpms_client.Common.Constants;
using icpms_client.Network.DTO.Gate;
using icpms_client.Network.Response;
using icpms_client.Network.Response.Home;
using icpms_client.Network.Services.Home;
using icpms_client.Services.ExternalServices;
using icpms_client.Tools.VideoPlayer;
using VehicleDetection.Contracts;

namespace icpms_client.Pages.Screens.ViewModels;

public class HomeScreenViewModel : INotifyPropertyChanged
{
    public ObservableCollection<VehicleDetectedEvent> DetectedVehicles { get; } = new();

    private readonly HomeScreenService _homeScreenService;

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

    private string _entranceDetectedVehicleNo = "--/----";
    private bool _entranceIsMember;
    private bool _entranceIsVip;

    private string _exitDetectedVehicleNo = "--/----";
    private bool _exitIsMember;
    private bool _exitIsVip;

    
    

    private ImageSource? _entranceSnapshotImage;
    private ImageSource? _exitSnapshotImage;
    
    private static ImageSource? _placeholderImage;
    private static bool _placeholderLoadAttempted;

    private bool _isSnapshotModalOpen;
    private ImageSource? _modalSnapshotImage;
    private string? _modalSnapshotTitle;
    
    private bool _entranceHasSnapshot;
    private bool _exitHasSnapshot;
    
    private SnapshotSide _activeSnapshotSide;
    

    public HomeScreenViewModel(HomeScreenService homeScreenService, VehicleDetectionRealtimeService vehicleService)
    {
        _homeScreenService = homeScreenService;
        
        vehicleService.VehicleDetected += OnVehicleDetected;

        _entranceSnapshotImage = GetPlaceholderImage();
        _exitSnapshotImage = GetPlaceholderImage();
        
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
        set { _exitIsMember = value; OnPropertyChanged(); }
    }

    public bool ExitIsVip
    {
        get => _exitIsVip;
        set { _exitIsVip = value; OnPropertyChanged(); }
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

    public async Task InitializeAsync()
    {
        LoadError = null;
        
        IsPreloadLoading = true;

        var preloadTask = LoadPreloadAsync();

        await Task.WhenAll(preloadTask);
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

        if (!string.IsNullOrWhiteSpace(_entranceAnprUrl)) EntranceAnprPlayer?.Play(_entranceAnprUrl);
        if (!string.IsNullOrWhiteSpace(_entranceCctvUrl)) EntranceCctvPlayer?.Play(_entranceCctvUrl);
        if (!string.IsNullOrWhiteSpace(_exitAnprUrl)) ExitAnprPlayer?.Play(_exitAnprUrl);
        if (!string.IsNullOrWhiteSpace(_exitCctvUrl)) ExitCctvPlayer?.Play(_exitCctvUrl);
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

    private Task ConfirmPaymentAsync()
    {
        IsPaymentComplete = true;
        IsReadyToConfirmPayment = false;
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
        EntranceSnapshotImage = captured ?? GetPlaceholderImage();
        EntranceHasSnapshot = captured != null;
    }

    private void CaptureExitSnapshot()
    {
        var captured = CaptureSnapshotImage(ExitAnprPlayer);
        ExitSnapshotImage = captured ?? GetPlaceholderImage();
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
    
    
    private static ImageSource? GetPlaceholderImage()
    {
        if (_placeholderLoadAttempted) return _placeholderImage;
        _placeholderLoadAttempted = true;

        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri("pack://application:,,,/Assets/Images/no-image.jpg");
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            _placeholderImage = bitmap;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load placeholder image: {ex.Message}");
            _placeholderImage = null;
        }

        return _placeholderImage;
    }
    
    private void OnVehicleDetected(VehicleDetectedEvent evt)
{
    Application.Current.Dispatcher.Invoke(() =>
    {
        DetectedVehicles.Insert(0, evt);

        switch (evt.DeviceIp.ToLowerInvariant())
        {
            case "entrance":
                ApplyEntranceDetection(evt);
                break;
            case "exit":
                ApplyExitDetection(evt);
                break;
            default:
                Console.WriteLine($"Vehicle event from unrecognized source '{evt.DeviceIp}' - check camera httpHosts config.");
                break;
        }
    });
}

    private void ApplyEntranceDetection(VehicleDetectedEvent evt)
    {
        EntranceDetectedVehicleNo = evt.LicensePlate;

        // Membership/VIP status isn't in the ANPR event itself - look it up here if needed:
        // var member = await _homeScreenService.LookupMemberAsync(evt.LicensePlate);
        // EntranceIsMember = member?.IsMember ?? false;
        // EntranceIsVip = member?.IsVip ?? false;

        CaptureEntranceSnapshot();
    }

    private void ApplyExitDetection(VehicleDetectedEvent evt)
    {
        ExitDetectedVehicleNo = evt.LicensePlate;

        CaptureExitSnapshot();

        // Hook into your existing exit flow here, e.g.:
        // ExitPlateMatched = await _homeScreenService.CheckPlateMatchAsync(evt.LicensePlate);
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