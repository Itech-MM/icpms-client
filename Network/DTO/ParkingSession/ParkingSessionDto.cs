namespace icpms_client.Network.DTO.ParkingSession;

public class ParkingSessionDto : CommonDto
{
    public long? VehicleId { get; set; }
    public string? PlateNumber { get; set; }
    public long? EntryGateId { get; set; }
    public string? EntryGateName { get; set; }
    public long? ExitGateId { get; set; }
    public string? ExitGateName { get; set; }
    public long? ParkingSlotId { get; set; }
    public string? ParkingSlotNumber { get; set; }
    public long? TariffId { get; set; }
    public string? TariffName { get; set; }
    public long? OperatorId { get; set; }
    public string? OperatorName { get; set; }
    public string? EntryTime { get; set; }
    public string? EntryPhotoUrl { get; set; }
    public string? ExitTime { get; set; }
    public string? ExitPhotoUrl { get; set; }
    public decimal? TotalAmount { get; set; }
    public int? Status { get; set; } = 1;
    public string? StatusDesc { get; set; }
    public string? SiteName { get; set; }
    public long? ParkingAreaId { get; set; }
    public string? ParkingAreaName { get; set; }
}