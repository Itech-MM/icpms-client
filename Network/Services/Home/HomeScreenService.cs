using icpms_client.Network.DTO.Shift;
using icpms_client.Network.Response;
using icpms_client.Network.Response.Home;
using icpms_client.Network.Session;

namespace icpms_client.Network.Services.Home;

public class HomeScreenService: ApiService
{
    public async Task<Response.Response?> GetHomeScreenPreload()
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

            var response = await ApiClient.GetAsync<HomeScreenPreloadResponse>("home/preload", true, accessToken);
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