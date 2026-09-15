using System.ComponentModel;
using System.Runtime.CompilerServices;
using icpms_client.Common.UI;
using icpms_client.Network.DTO.ParkingSession;

namespace icpms_client.Pages.Screens.Sections.ViewModels;

public class RecentVisitorItemViewModel : ScreenViewModelBase
{
    public long Id { get; }

    private string _plateNumber;
    public string PlateNumber
    {
        get => _plateNumber;
        private set { _plateNumber = value; OnPropertyChanged(); }
    }

    private bool _isUnknownPlate;
    public bool IsUnknownPlate
    {
        get => _isUnknownPlate;
        private set { _isUnknownPlate = value; OnPropertyChanged(); }
    }

    private string _gateName;
    public string GateName
    {
        get => _gateName;
        private set { _gateName = value; OnPropertyChanged(); }
    }

    private string _time;
    public string Time
    {
        get => _time;
        private set { _time = value; OnPropertyChanged(); }
    }

    private string _statusDesc;
    public string StatusDesc
    {
        get => _statusDesc;
        private set { _statusDesc = value; OnPropertyChanged(); }
    }

    private bool _isMember;
    public bool IsMember
    {
        get => _isMember;
        private set { _isMember = value; OnPropertyChanged(); }
    }

    private bool _isVip;
    public bool IsVip
    {
        get => _isVip;
        private set { _isVip = value; OnPropertyChanged(); }
    }

    public RecentVisitorItemViewModel(RecentVisitorDto dto)
    {
        Id = dto.Id;
        PlateNumber = dto.PlateNumber ?? "-";
        IsUnknownPlate = dto.IsUnknownPlate;
        GateName = dto.GateName ?? "-";
        Time = dto.Time ?? "-";
        StatusDesc = dto.StatusDesc ?? "-";
        IsMember = dto.IsMember;
        IsVip = dto.IsVip;
    }

    public void UpdateFrom(RecentVisitorDto dto)
    {
        PlateNumber = dto.PlateNumber ?? "-";
        IsUnknownPlate = dto.IsUnknownPlate;
        GateName = dto.GateName ?? "-";
        Time = dto.Time ?? "-";
        StatusDesc = dto.StatusDesc ?? "-";
        IsMember = dto.IsMember;
        IsVip = dto.IsVip;
    }

}