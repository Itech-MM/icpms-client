namespace icpms_client.Network.DTO.AuditLogs;

public class VehicleAlertDto: CommonDto
{
    public int AlertType { get; set; }
    public string? AlertTypeDesc { get; set; }
    public string? PlateNumber { get; set; }
    public long? VehicleId { get; set; }
    public long? MemberId { get; set; }
    public long? SessionId { get; set; }
    public long? GateId { get; set; }
    public long? OperatorId { get; set; }
    public string? Message { get; set; }
}