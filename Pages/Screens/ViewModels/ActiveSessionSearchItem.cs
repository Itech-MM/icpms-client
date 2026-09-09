namespace icpms_client.Pages.Screens.ViewModels;

public class ActiveSessionSearchItem
{
    public string PlateNumber { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
    public bool IsMember { get; set; }
    public string StatusDesc { get; set; } = string.Empty;
}