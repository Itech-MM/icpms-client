using icpms_client.Network.DTO.Gate;
using icpms_client.Network.Response;
using icpms_client.Network.Session;
using log4net;

namespace icpms_client.Network.Services.Gate;

public class GateService: ApiService
{
    private readonly ILog _log = LogManager.GetLogger(typeof(GateService));
    
    public async Task<Response.Response?> GetGateDevices()
    {
        try
        {
            var accessToken = UserSession.CurrentUser.CurrentAuth?.AccessToken;
            if (string.IsNullOrEmpty(accessToken))
            {
                _log.Error("Access token is null or empty");
                return new BaseErrorResponse<string>
                {
                    Success = false,
                    Message = "No active session/access token found.",
                    Data = "No active session/access token found."
                };
            }

            var response = await ApiClient.GetAsync<List<GateDeviceDto>>("gate/devices", true, accessToken);
            return response;
        }
        catch (Exception ex)
        {
            
            _log.Error($"GetGateDevices Error: {ex.Message}");
            var error = new BaseErrorResponse<string>
            {
                Success = false,
                Message = ex.Message,
                Data = ex.Message
            };
            return error;
        }
    }
}