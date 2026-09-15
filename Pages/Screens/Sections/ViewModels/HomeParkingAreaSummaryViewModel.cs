using icpms_client.Common.UI;
using icpms_client.Network.DTO.ParkingArea;
using icpms_client.Network.Response;
using icpms_client.Network.Services.ParkingArea;

namespace icpms_client.Pages.Screens.Sections.ViewModels;

public enum CapacityChipStatus
{
    Success,
    Warning,
    Danger
}

public class HomeParkingAreaSummaryViewModel : ScreenViewModelBase
{
    private readonly ParkingAreaService _parkingAreaService;

    private bool _isParkingAreaLoading = true;
    private string? _parkingAreaLoadError;

    private int _areaCapacityPercentage;
    private int _totalSlot;
    private int _totalAvailableSlot;
    private int _vipSlot;
    private int _totalAvailableVipSlot;
    private int _normalSlot;
    private int _totalAvailableNormalSlot;

    public HomeParkingAreaSummaryViewModel(ParkingAreaService parkingAreaService)
    {
        _parkingAreaService = parkingAreaService;
    }

    public async Task LoadParkingAreaSummaryAsync()
    {
        ParkingAreaLoadError = null;

        IsParkingAreaLoading = true;

        var preloadTask = LoadParkingAreaAsync();

        await Task.WhenAll(preloadTask);
    }

    public async Task Refresh()
    {
        await LoadParkingAreaAsync();
    }

    private void ApplyParkingAreaData(RealtimeParkingAreaDto data)
    {
        TotalSlot = data.TotalSlot;
        TotalAvailableSlot = data.AvailableTotalSlot;
        VipSlot = data.VipSlot;
        TotalAvailableVipSlot = data.AvailableTotalVipSlot;
        NormalSlot = data.NormalSlot;
        TotalAvailableNormalSlot = data.AvailableTotalNormalSlot;

        AreaCapacityPercentage = TotalSlot > 0
            ? (int)Math.Round((double)OccupiedTotalSlot / TotalSlot * 100)
            : 0;

        OnPropertyChanged(nameof(OccupiedTotalSlot));
        OnPropertyChanged(nameof(OccupiedNormalSlot));
        OnPropertyChanged(nameof(OccupiedVipSlot));
        OnPropertyChanged(nameof(TotalSlotSummaryText));
        OnPropertyChanged(nameof(NormalSlotSummaryText));
        OnPropertyChanged(nameof(VipSlotSummaryText));
        OnPropertyChanged(nameof(AreaCapacityStatus));
    }

    private async Task LoadParkingAreaAsync()
    {
        try
        {
            var response = await _parkingAreaService.GetRealtimeParkingAreaData();

            if (response is BaseResponse<RealtimeParkingAreaDto> { Success: true, Data: not null } success)
            {
                ApplyParkingAreaData(success.Data);
            }
            else
            {
                ParkingAreaLoadError = response.Message ?? "Failed to load parking area data.";
            }
        }
        catch (Exception ex)
        {
            ParkingAreaLoadError = ex.Message;
        }
        finally
        {
            IsParkingAreaLoading = false;
        }
    }

    public bool IsParkingAreaLoading
    {
        get => _isParkingAreaLoading;
        private set { _isParkingAreaLoading = value; OnPropertyChanged(); }
    }

    public string? ParkingAreaLoadError
    {
        get => _parkingAreaLoadError;
        private set { _parkingAreaLoadError = value; OnPropertyChanged(); }
    }

    public int AreaCapacityPercentage
    {
        get => _areaCapacityPercentage;
        set { _areaCapacityPercentage = value; OnPropertyChanged(); }
    }

    public int TotalSlot
    {
        get => _totalSlot;
        set { _totalSlot = value; OnPropertyChanged(); }
    }

    public int TotalAvailableSlot
    {
        get => _totalAvailableSlot;
        set { _totalAvailableSlot = value; OnPropertyChanged(); }
    }

    public int NormalSlot
    {
        get => _normalSlot;
        set { _normalSlot = value; OnPropertyChanged(); }
    }

    public int TotalAvailableNormalSlot
    {
        get => _totalAvailableNormalSlot;
        set { _totalAvailableNormalSlot = value; OnPropertyChanged(); }
    }

    public int VipSlot
    {
        get => _vipSlot;
        set { _vipSlot = value; OnPropertyChanged(); }
    }

    public int TotalAvailableVipSlot
    {
        get => _totalAvailableVipSlot;
        set { _totalAvailableVipSlot = value; OnPropertyChanged(); }
    }

    public int OccupiedTotalSlot => TotalSlot - TotalAvailableSlot;
    public int OccupiedNormalSlot => NormalSlot - TotalAvailableNormalSlot;
    public int OccupiedVipSlot => VipSlot - TotalAvailableVipSlot;

    public string TotalSlotSummaryText => $"{OccupiedTotalSlot}/{TotalSlot}";
    public string NormalSlotSummaryText => $"{OccupiedNormalSlot}/{NormalSlot}";
    public string VipSlotSummaryText => $"{OccupiedVipSlot}/{VipSlot}";

    public CapacityChipStatus AreaCapacityStatus =>
        AreaCapacityPercentage >= 100 ? CapacityChipStatus.Danger :
        AreaCapacityPercentage >= 70 ? CapacityChipStatus.Warning :
        CapacityChipStatus.Success;
}