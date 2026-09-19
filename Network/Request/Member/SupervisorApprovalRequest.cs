using Newtonsoft.Json;

namespace icpms_client.Network.Request.Member;

public class SupervisorApprovalRequest
{
    [JsonProperty("authMethod")]
    public int AuthMethod { get; set; }

    [JsonProperty("username")]
    public string? Username { get; set; }

    [JsonProperty("password")]
    public string? Password { get; set; }

    [JsonProperty("credential")]
    public string? Credential { get; set; }
}