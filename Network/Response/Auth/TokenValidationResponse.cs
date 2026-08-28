namespace icpms_client.Network.Response.Auth;

public class TokenValidationResponse
{
    public bool Valid { get; set; }
    public string Username { get; set; }
    public List<string> Roles { get; set; }
    public string Reason { get; set; }
    public bool StartShift { get; set; }

    public TokenValidationResponse() { }

    public TokenValidationResponse(bool valid, string username, List<string> roles, string reason, bool startShift)
    {
        Valid = valid;
        Username = username;
        Roles = roles;
        Reason = reason;
        StartShift = startShift;
    }
}