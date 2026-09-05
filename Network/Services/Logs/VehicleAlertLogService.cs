using icpms_client.Network.DTO;
using icpms_client.Network.DTO.AuditLogs;
using icpms_client.Network.Response;
using icpms_client.Network.Session;
using log4net;

namespace icpms_client.Network.Services.Logs;

public class VehicleAlertLogService: ApiService
{
    private readonly ILog _log = LogManager.GetLogger(typeof(VehicleAlertLogService));
    
    public async Task<Response.Response?> SearchLogs(int pageNo = 1)
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

            var response = await ApiClient.GetAsync<SearchResultDto<VehicleAlertDto>>($"vehicle-alerts/get-active-alerts?page={pageNo}", true, accessToken);
            return response;
        }
        catch (Exception ex)
        {
            
            _log.Error($"GetVehicleDetailByPlateNumber Error: {ex.Message}");
            var error = new BaseErrorResponse<string>
            {
                Success = false,
                Message = ex.Message,
                Data = ex.Message
            };
            return error;
        }
    }
    
    public async Task<Response.Response?> UpdateStatus(long id)
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

            var response = await ApiClient.PatchAsync<Response.Response>($"vehicle-alerts/update-status/{id}/status/2", requiresAuth: true,  token: accessToken);
            return response;
        }
        catch (Exception ex)
        {
            
            _log.Error($"GetVehicleDetailByPlateNumber Error: {ex.Message}");
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