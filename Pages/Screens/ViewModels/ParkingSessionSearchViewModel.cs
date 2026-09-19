using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;
using icpms_client.Common.Command;
using icpms_client.Common.ContextData;
using icpms_client.Common.UI;
using icpms_client.Network.DTO;
using icpms_client.Network.DTO.ParkingSession;
using icpms_client.Network.Request.ParkingSession;
using icpms_client.Network.Response;
using icpms_client.Network.Services.Visitor;
using icpms_client.Services.UIServices;
using icpms_client.ViewModels.Wrappers;
using log4net;

namespace icpms_client.Pages.Screens.ViewModels;

public class ParkingSessionSearchViewModel : ScreenViewModelBase
{
    private readonly ILog _log = LogManager.GetLogger(nameof(ParkingSessionSearchViewModel));
    private readonly VisitorService _visitorService;

    private string? _plateNumber;
    private DateTime? _fromDate;
    private DateTime? _toDate;
    private LabelValueWrapper? _selectedStatus;

    private bool _isLoading;
    private string? _loadError;

    private int _pageNo = 1;
    private int _totalPage;
    private int _totalRecords;
    private bool _hasNextPage;

    private bool _isModalOpen;
    private bool _isDetailLoading;
    private ParkingSessionDto? _selectedDetail;

    private BitmapImage? _entryVehiclePhoto;
    private bool _isEntryVehicleLoading;
    private BitmapImage? _entryPlatePhoto;
    private bool _isEntryPlateLoading;
    private BitmapImage? _exitVehiclePhoto;
    private bool _isExitVehicleLoading;
    private BitmapImage? _exitPlatePhoto;
    private bool _isExitPlateLoading;
    
    public ObservableCollection<LabelValueWrapper> StatusOptions { get; } = new()
    {
        new LabelValueWrapper { Label = "-- All --", Value = "-1" },
        new LabelValueWrapper { Label = "Active", Value = "1" },
        new LabelValueWrapper { Label = "Completed", Value = "2" },
        new LabelValueWrapper { Label = "Cancelled", Value = "3" }
    };

    public ObservableCollection<ParkingSessionDto> Results { get; } = new();

    public ParkingSessionSearchViewModel(VisitorService visitorService)
    {
        _visitorService = visitorService;
        _selectedStatus = StatusOptions[0];

        SearchCommand = new RelayCommand(async void (_) => await ExecuteSearchAsync(1));
        ResetCommand = new RelayCommand(_ => ResetFilters());
        NextPageCommand = new RelayCommand(async void (_) => await ExecuteSearchAsync(_pageNo + 1), _ => HasNextPage && !IsLoading);
        PrevPageCommand = new RelayCommand(async void (_) => await ExecuteSearchAsync(_pageNo - 1), _ => HasPrevPage && !IsLoading);
        
        OpenDetailCommand = new RelayCommand(async void (param) =>
        {
            if (param is ParkingSessionDto { Id: > 0 } dto)
                await OpenDetailAsync(dto.Id);
        });
        CloseModalCommand = new RelayCommand(_ => CloseModal());
    }

    private async Task OpenDetailAsync(long id)
{
    IsModalOpen = true;
    IsDetailLoading = true;
    SelectedDetail = null;
    ResetPhotos();

    try
    {
        var response = await _visitorService.GetVisitorDetail(id);

        if (response is BaseResponse<ParkingSessionDto> { Success: true, Data: not null } success)
        {
            SelectedDetail = success.Data;
            LoadPhotos(success.Data);
        }
        else
        {
            var message = response?.Message ?? "Failed to load parking session detail.";
            ToastService.ShowError(message);
            CloseModal();
        }
    }
    catch (Exception ex)
    {
        _log.Error($"GetVisitorDetail Error: {ex.Message}");
        ToastService.ShowError(ex.Message);
        CloseModal();
    }
    finally
    {
        IsDetailLoading = false;
    }
}

private void CloseModal()
{
    IsModalOpen = false;
    SelectedDetail = null;
    ResetPhotos();
}

private void ResetPhotos()
{
    EntryVehiclePhoto = null;
    EntryPlatePhoto = null;
    ExitVehiclePhoto = null;
    ExitPlatePhoto = null;
    IsEntryVehicleLoading = false;
    IsEntryPlateLoading = false;
    IsExitVehicleLoading = false;
    IsExitPlateLoading = false;
}

private void LoadPhotos(ParkingSessionDto detail)
{
    LoadPhoto(detail.EntryPhotoUrl, img => EntryVehiclePhoto = img, loading => IsEntryVehicleLoading = loading);
    LoadPhoto(detail.EntryPlatePhotoUrl, img => EntryPlatePhoto = img, loading => IsEntryPlateLoading = loading);
    LoadPhoto(detail.ExitPhotoUrl, img => ExitVehiclePhoto = img, loading => IsExitVehicleLoading = loading);
    LoadPhoto(detail.ExitPlatePhotoUrl, img => ExitPlatePhoto = img, loading => IsExitPlateLoading = loading);
}

private void LoadPhoto(string? url, Action<BitmapImage?> setImage, Action<bool> setLoading)
{
    if (string.IsNullOrWhiteSpace(url))
    {
        setImage(null);
        setLoading(false);
        return;
    }

    setLoading(true);

    Task.Run(() =>
    {
        var ftp = CommonData.FtpUtil;
        BitmapImage? image = null;

        try
        {
            image = ftp?.GetImageFromFtp($"{CommonData.FtpFolderPath}/{url}");
        }
        catch (Exception ex)
        {
            _log.Error($"GetImageFromFtp Error ({url}): {ex.Message}");
        }

        Application.Current.Dispatcher.Invoke(() =>
        {
            setImage(image);
            setLoading(false);
        });
    });
}
    
    public string? PlateNumber
    {
        get => _plateNumber;
        set { _plateNumber = value; OnPropertyChanged(); }
    }

    public DateTime? FromDate
    {
        get => _fromDate;
        set { _fromDate = value; OnPropertyChanged(); }
    }

    public DateTime? ToDate
    {
        get => _toDate;
        set { _toDate = value; OnPropertyChanged(); }
    }

    public LabelValueWrapper? SelectedStatus
    {
        get => _selectedStatus;
        set { _selectedStatus = value; OnPropertyChanged(); }
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            _isLoading = value;
            OnPropertyChanged();
            NextPageCommand.RaiseCanExecuteChanged();
            PrevPageCommand.RaiseCanExecuteChanged();
        }
    }

    public string? LoadError
    {
        get => _loadError;
        private set { _loadError = value; OnPropertyChanged(); }
    }

    public bool HasResults => Results.Count > 0;

    public int PageNo
    {
        get => _pageNo;
        private set { _pageNo = value; OnPropertyChanged(); OnPropertyChanged(nameof(PageInfoText)); }
    }

    public int TotalPage
    {
        get => _totalPage;
        private set { _totalPage = value; OnPropertyChanged(); OnPropertyChanged(nameof(PageInfoText)); }
    }

    public int TotalRecords
    {
        get => _totalRecords;
        private set { _totalRecords = value; OnPropertyChanged(); }
    }

    public bool HasNextPage
    {
        get => _hasNextPage;
        private set
        {
            _hasNextPage = value;
            OnPropertyChanged();
            NextPageCommand.RaiseCanExecuteChanged();
        }
    }

    public bool HasPrevPage => PageNo > 1;

    public string PageInfoText => $"Page {PageNo} of {Math.Max(TotalPage, 1)}";

    public bool IsModalOpen
    {
        get => _isModalOpen;
        private set { _isModalOpen = value; OnPropertyChanged(); }
    }

    public bool IsDetailLoading
    {
        get => _isDetailLoading;
        private set { _isDetailLoading = value; OnPropertyChanged(); }
    }

    public ParkingSessionDto? SelectedDetail
    {
        get => _selectedDetail;
        private set { _selectedDetail = value; OnPropertyChanged(); }
    }

    public BitmapImage? EntryVehiclePhoto
    {
        get => _entryVehiclePhoto;
        private set { _entryVehiclePhoto = value; OnPropertyChanged(); }
    }
    public bool IsEntryVehicleLoading
    {
        get => _isEntryVehicleLoading;
        private set { _isEntryVehicleLoading = value; OnPropertyChanged(); }
    }

    public BitmapImage? EntryPlatePhoto
    {
        get => _entryPlatePhoto;
        private set { _entryPlatePhoto = value; OnPropertyChanged(); }
    }
    public bool IsEntryPlateLoading
    {
        get => _isEntryPlateLoading;
        private set { _isEntryPlateLoading = value; OnPropertyChanged(); }
    }

    public BitmapImage? ExitVehiclePhoto
    {
        get => _exitVehiclePhoto;
        private set { _exitVehiclePhoto = value; OnPropertyChanged(); }
    }
    public bool IsExitVehicleLoading
    {
        get => _isExitVehicleLoading;
        private set { _isExitVehicleLoading = value; OnPropertyChanged(); }
    }

    public BitmapImage? ExitPlatePhoto
    {
        get => _exitPlatePhoto;
        private set { _exitPlatePhoto = value; OnPropertyChanged(); }
    }
    public bool IsExitPlateLoading
    {
        get => _isExitPlateLoading;
        private set { _isExitPlateLoading = value; OnPropertyChanged(); }
    }

    public RelayCommand OpenDetailCommand { get; }
    public RelayCommand CloseModalCommand { get; }
    
    public RelayCommand SearchCommand { get; }
    public RelayCommand ResetCommand { get; }
    public RelayCommand NextPageCommand { get; }
    public RelayCommand PrevPageCommand { get; }

    private void ResetFilters()
    {
        PlateNumber = null;
        FromDate = null;
        ToDate = null;
        SelectedStatus = StatusOptions[0];
        _ = ExecuteSearchAsync(1);
    }

    private async Task ExecuteSearchAsync(int pageNo)
    {
        if (pageNo < 1) return;

        IsLoading = true;
        LoadError = null;

        try
        {
            var statusValue = -1;
            if (SelectedStatus != null && int.TryParse(SelectedStatus.Value, out var parsedStatus))
                statusValue = parsedStatus;

            var request = new ParkingSessionSearchRequest
            {
                PlateNumber = string.IsNullOrWhiteSpace(PlateNumber) ? null : PlateNumber,
                Status = statusValue,
                FromDate = FromDate?.ToString("yyyy-MM-dd 00:00:00"),
                ToDate = ToDate?.ToString("yyyy-MM-dd 23:59:59"),
                PageNo = pageNo
            };

            var response = await _visitorService.SearchSession(request);

            if (response is BaseResponse<SearchResultDto<ParkingSessionDto>> { Success: true, Data: not null } success)
            {
                var page = success.Data;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Results.Clear();
                    foreach (var item in page.Results)
                        Results.Add(item);
                    OnPropertyChanged(nameof(HasResults));
                });

                PageNo = (page.PageNo ?? pageNo) + 1;
                TotalPage = page.TotalPage ?? 1;
                TotalRecords = page.TotalRecords ?? page.Results.Count;
                HasNextPage = page.HasNextPage ?? false;
            }
            else
            {
                var message = response?.Message ?? "Failed to search parking sessions.";
                LoadError = message;
                ToastService.ShowError(message);
            }
        }
        catch (Exception ex)
        {
            _log.Error($"SearchSession Error: {ex.Message}");
            LoadError = ex.Message;
            ToastService.ShowError(ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    public Task InitializeAsync() => ExecuteSearchAsync(1);

}