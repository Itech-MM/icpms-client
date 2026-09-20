namespace icpms_client.Pages.Screens.ViewModels;

public class DeviceSummaryTile : ObservableBase
{
    private int _count;
    private bool _isSelected;

    public DeviceSummaryTile(string key, string label)
    {
        Key = key;
        Label = label;
    }

    public string Key { get; }

    public string Label { get; }

    public int Count
    {
        get => _count;
        set => SetField(ref _count, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetField(ref _isSelected, value);
    }
}