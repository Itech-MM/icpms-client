using Newtonsoft.Json;

namespace icpms_client.Network.Request.Member;

public class MemberSearchRequest
{
    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("phoneNumber")]
    public string? PhoneNumber { get; set; }

    [JsonProperty("status")]
    public int? Status { get; set; }

    [JsonProperty("pageNo")]
    public int PageNo { get; set; } = 1;
    
    [JsonProperty("keyword")]
    public string? Keyword {get; set;}
}