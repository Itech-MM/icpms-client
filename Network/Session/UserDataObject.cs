using icpms_client.Network.DTO.User;
using icpms_client.Network.Response.Auth;

namespace icpms_client.Network.Session;

public class UserDataObject
{
    public AuthResponse? CurrentAuth { get; set; }
}