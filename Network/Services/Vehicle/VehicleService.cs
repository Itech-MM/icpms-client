using icpms_client.Network.DTO;
using icpms_client.Network.DTO.Vehicle;
using icpms_client.Network.Request.Vehicle;
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
    
    public async Task<Response.Response?> SearchVehicles(VehicleSearchRequest request)
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

            var query = new List<string>();

            if (!string.IsNullOrWhiteSpace(request.PlateNumber))
                query.Add($"plateNumber={Uri.EscapeDataString(request.PlateNumber)}");

            if (request.Status.HasValue)
                query.Add($"status={request.Status.Value}");

            if (request.FromSession.HasValue)
                query.Add($"fromSession={request.FromSession.Value}");

            query.Add($"page={Math.Max(request.PageNo - 1, 0)}");

            var url = $"vehicle/search?{string.Join("&", query)}";

            var response = await ApiClient.GetAsync<SearchResultDto<VehicleDto>>(url, true, accessToken);
            return response;
        }
        catch (Exception ex)
        {
            _log.Error($"SearchVehicles Error: {ex.Message}");
            return new BaseErrorResponse<string>
            {
                Success = false,
                Message = ex.Message,
                Data = ex.Message
            };
        }
    }
}