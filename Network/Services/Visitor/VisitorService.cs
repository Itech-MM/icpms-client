using icpms_client.Network.DTO.ParkingSession;
using icpms_client.Network.Request.Visitor;
using icpms_client.Network.Response;
using icpms_client.Network.Session;

namespace icpms_client.Network.Services.Visitor;

public class VisitorService: ApiService
{
    public async Task<Response.Response?> SaveEntryVisitor(VisitorEntryRequest request)
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

            var response = await ApiClient.PostAsync<ParkingSessionDto>("visitors/entry", request, true, accessToken);
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