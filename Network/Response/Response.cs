namespace icpms_client.Network.Response;

public class Response
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}