namespace icpms_client.Network.Request.Vehicle;

public class VehicleSearchRequest
{
    public string? PlateNumber { get; set; }
    public int? Status { get; set; }
    public int? FromSession { get; set; }
    public int PageNo { get; set; } = 1;
}