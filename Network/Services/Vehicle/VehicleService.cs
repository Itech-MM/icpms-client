using icpms_client.Network.Response;
using icpms_client.Network.Response.Vehicle;
using icpms_client.Network.Session;
using log4net;

namespace icpms_client.Network.Services.Vehicle;

public class VehicleService: ApiService
{
    private readonly ILog _log = LogManager.GetLogger(typeof(VehicleService));
    
    public async Task<Response.Response?> GetVehicleDetailByPlateNumber(string plateNumber)
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

            var response = await ApiClient.GetAsync<VehicleDetailResponse>($"vehicle/get-by-number?number={Uri.EscapeDataString(plateNumber)}", true, accessToken);
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