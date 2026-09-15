using Newtonsoft.Json;

namespace icpms_client.Network.Request.Member;

public class SupervisorApprovalRequest
{
    [JsonProperty("username")]
    public string? Username { get; set; }
    
    [JsonProperty("password")]
    public string? Password { get; set; }
}