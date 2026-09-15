using Newtonsoft.Json;

namespace icpms_client.Network.Request.ParkingSession;

public class ParkingSessionSearchRequest
{
    [JsonProperty("plateNumber")]
    public string? PlateNumber { get; set;} = string.Empty;
    [JsonProperty("status")]
    public int? Status { get; set; } = -1;
    [JsonProperty("fromDate")]
    public string? FromDate { get; set;} = string.Empty; // yyyy-MM-dd HH:mm:ss
    [JsonProperty("toDate")]
    public string? ToDate { get; set;} = string.Empty; // yyyy-MM-dd HH:mm:ss
    [JsonProperty("pageNo")]
    public int? PageNo { get; set;} = 1;
}