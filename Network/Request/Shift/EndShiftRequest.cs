using Newtonsoft.Json;

namespace icpms_client.Network.Request.Shift;

public class EndShiftRequest
{
    [JsonProperty("closingCash")]
    public decimal? ClosingCash { get; set; }
    
    [JsonProperty("remark")]
    public string? Remark { get; set; }
}