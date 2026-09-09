namespace icpms_client.Network.DTO.ParkingSession;

public class RecentVisitorDto
{
    public long Id { get; set; }
    public string? PlateNumber { get; set; }
    public bool IsUnknownPlate { get; set; }
    public string? GateName { get; set; }
    public string? Time { get; set; }
    public int? Status { get; set; }
    public string? StatusDesc { get; set; }
    public bool IsMember { get; set; }
    public bool IsVip { get; set; }
}