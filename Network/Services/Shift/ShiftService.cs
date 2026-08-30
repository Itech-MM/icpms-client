using icpms_client.Network.Core;
using icpms_client.Network.DTO.Shift;
using icpms_client.Network.Request.Shift;
using icpms_client.Network.Response;
using icpms_client.Network.Session;
using Microsoft.Extensions.DependencyInjection;

namespace icpms_client.Network.Services.Shift;

public class ShiftService: ApiService
{

    public async Task<Response.Response?> StartShift(StartShiftRequest request)
    {
        try
        {
            var response = await ApiClient.PostAsync<ShiftDto>("shifts/start", request, true, UserSession.CurrentUser.CurrentAuth.AccessToken);
            return response;
        }
        catch (Exception ex)
        {
            var error = new BaseErrorResponse<string>
            {
                Success = false,
                Message = ex.Message,
                Data = ex.Message
            };
            return error;
        }
    }
    
    public async Task<Response.Response?> EndShift(EndShiftRequest request)
    {
        try
        {
            var accessToken = UserSession.CurrentUser?.CurrentAuth?.AccessToken;
            if (string.IsNullOrEmpty(accessToken))
            {
                return new BaseErrorResponse<string>
                {
                    Success = false,
                    Message = "No active session/access token found.",
                    Data = "No active session/access token found."
                };
            }

            var response = await ApiClient.PostAsync<ShiftDto>("shifts/end", request, true, accessToken);
            return response;
        }
        catch (Exception ex)
        {
            var error = new BaseErrorResponse<string>
            {
                Success = false,
                Message = ex.Message,
                Data = ex.Message
            };
            return error;
        }
    }
    
    public async Task<Response.Response?> GetShiftSummary()
    {
        try
        {
            var accessToken = UserSession.CurrentUser?.CurrentAuth?.AccessToken;
            if (string.IsNullOrEmpty(accessToken))
            {
                return new BaseErrorResponse<string>
                {
                    Success = false,
                    Message = "No active session/access token found.",
                    Data = "No active session/access token found."
                };
            }

            var response = await ApiClient.GetAsync<ShiftSummaryDto>("shifts/summary", true, accessToken);
            return response;
        }
        catch (Exception ex)
        {
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