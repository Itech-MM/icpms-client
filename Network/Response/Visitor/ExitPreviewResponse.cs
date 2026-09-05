namespace icpms_client.Network.Response.Visitor;

public class ExitPreviewResponse
{
    public long SessionId { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public string EntryTime { get; set; } = string.Empty;
    public long DurationMinutes { get; set; } = 0;
    public string TariffName { get; set; } = string.Empty;
    public decimal AmountDue { get; set; } = 0;
}