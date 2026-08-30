using System.ComponentModel;
using System.Runtime.CompilerServices;
using icpms_client.Network.DTO.Shift;
using icpms_client.Network.Response;
using icpms_client.Network.Services.Shift;

namespace icpms_client.Pages.Screens.Sections.ViewModels;

public class HomeShiftSummaryViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly ShiftService _shiftService = new();

    private bool _isShiftSummaryLoading = true;
    private string? _shiftSummaryLoadError;

    private string _shiftCode = string.Empty;
    private int _vehiclesIn;
    private int _vehiclesOut;
    private int _currentlyParked;
    private string _revenueCollectedText = "0";

    public async Task LoadShiftSummaryAsync()
    {
        ShiftSummaryLoadError = null;

        IsShiftSummaryLoading = true;

        var summaryTask = LoadShiftSummaryInternalAsync();

        await Task.WhenAll(summaryTask);
    }

    public async Task Refresh()
    {
        await LoadShiftSummaryInternalAsync();
    }

    private void ApplyShiftSummaryData(ShiftSummaryDto data)
    {
        ShiftCode = data.Code;
        VehiclesIn = data.TotalTransactions;
        VehiclesOut = data.TotalCompletedTransactions;
        CurrentlyParked = data.TotalIncompleteTransactions;
        RevenueCollectedText = data.TotalAmountDesc;

        OnPropertyChanged(nameof(ShiftSubtitle));
    }

    private async Task LoadShiftSummaryInternalAsync()
    {
        try
        {
            var response = await _shiftService.GetShiftSummary();

            if (response is BaseResponse<ShiftSummaryDto> { Success: true, Data: not null } success)
            {
                ApplyShiftSummaryData(success.Data);
            }
            else
            {
                ShiftSummaryLoadError = response?.Message ?? "Failed to load shift summary.";
            }
        }
        catch (Exception ex)
        {
            ShiftSummaryLoadError = ex.Message;
        }
        finally
        {
            IsShiftSummaryLoading = false;
        }
    }

    public bool IsShiftSummaryLoading
    {
        get => _isShiftSummaryLoading;
        private set { _isShiftSummaryLoading = value; OnPropertyChanged(); }
    }

    public string? ShiftSummaryLoadError
    {
        get => _shiftSummaryLoadError;
        private set { _shiftSummaryLoadError = value; OnPropertyChanged(); }
    }

    public string ShiftCode
    {
        get => _shiftCode;
        set { _shiftCode = value; OnPropertyChanged(); }
    }

    public int VehiclesIn
    {
        get => _vehiclesIn;
        set { _vehiclesIn = value; OnPropertyChanged(); }
    }

    public int VehiclesOut
    {
        get => _vehiclesOut;
        set { _vehiclesOut = value; OnPropertyChanged(); }
    }

    public int CurrentlyParked
    {
        get => _currentlyParked;
        set { _currentlyParked = value; OnPropertyChanged(); }
    }

    public string RevenueCollectedText
    {
        get => _revenueCollectedText;
        set { _revenueCollectedText = value; OnPropertyChanged(); }
    }

    public string ShiftSubtitle => string.IsNullOrEmpty(ShiftCode) ? "####" : $"{ShiftCode}";

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}