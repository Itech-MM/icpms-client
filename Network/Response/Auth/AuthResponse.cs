using System.Collections.Generic;

namespace icpms_client.Network.Response.Auth;

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public long ExpiresIn { get; set; }
    public bool StartShift { get; set; }

    public AuthResponse() { }

    public AuthResponse(
        string accessToken, 
        string refreshToken, 
        string username, 
        List<string> roles, 
        long expiresIn, 
        bool startShift)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        Username = username;
        Roles = roles;
        ExpiresIn = expiresIn;
        StartShift = startShift;
    }
}