using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using icpms_client.Common.Command;
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