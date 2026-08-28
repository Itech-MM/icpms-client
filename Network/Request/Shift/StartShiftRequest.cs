using Newtonsoft.Json;

namespace icpms_client.Network.Request.Shift;

public class StartShiftRequest
{
    [JsonProperty("openingCash")]
    public decimal? OpeningCash { get; set; }
    
    [JsonProperty("remark")]
    public string? Remark { get; set; }
}