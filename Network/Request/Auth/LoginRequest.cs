using Newtonsoft.Json;

namespace icpms_client.Network.Request.Auth;

public class LoginRequest
{
    [JsonProperty("username")]
    public string Username { get; set; } = string.Empty;
    
    [JsonProperty("password")]
    public string Password { get; set; } = string.Empty;
}