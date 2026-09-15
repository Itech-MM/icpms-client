using Newtonsoft.Json;

namespace icpms_client.Network.Request.Member;

public class TopUpBalanceRequest
{
    [JsonProperty("amount")]
    public decimal Amount { get; set; }

    [JsonProperty("remark")]
    public string? Remark { get; set; }
}