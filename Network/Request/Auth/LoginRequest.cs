using Newtonsoft.Json;

namespace icpms_client.Network.Request.Auth;

public class LoginRequest
{
    [JsonProperty("authMethod")]
    public int AuthMethod { get; set; }

    [JsonProperty("username")]
    public string Username { get; set; } = string.Empty;

    [JsonProperty("password")]
    public string Password { get; set; } = string.Empty;

    [JsonProperty("credential")]
    public string? Credential { get; set; }
}