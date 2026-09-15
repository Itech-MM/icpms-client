using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace icpms_client.State;

public class ShiftSummaryState : INotifyPropertyChanged
{
    private int _vehiclesIn;
    private int _vehiclesOut;
    private int _currentlyParked;
    private string _revenueCollectedText = "0";

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

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}