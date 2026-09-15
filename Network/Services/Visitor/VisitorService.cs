using icpms_client.Network.DTO;
using icpms_client.Network.DTO.ParkingSession;
using icpms_client.Network.Request.ParkingSession;
using icpms_client.Network.Request.Visitor;
using icpms_client.Network.Response;
using icpms_client.Network.Response.Visitor;
using icpms_client.Network.Session;
using log4net;

namespace icpms_client.Network.Services.Visitor;

public class VisitorService: ApiService
{
    private readonly ILog _log = LogManager.GetLogger(typeof(VisitorService));
    
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
    
    public async Task<Response.Response?> GetExitPreview(string plateNumber)
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

            var response = await ApiClient.GetAsync<ExitPreviewResponse>($"visitors/exit-preview?plateNumber={plateNumber}", true, accessToken);
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
    
    public async Task<Response.Response?> SaveExitVisitor(VisitorExitRequest request)
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

            var response = await ApiClient.PostAsync<VisitorExitResponse>("visitors/exit", request, true, accessToken);
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
    
    public async Task<Response.Response?> SearchRecentVisitors(int pageNo = 1)
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

            return await ApiClient.GetAsync<SearchResultDto<RecentVisitorDto>>(
                $"visitors/recent?page={pageNo}", true, accessToken);
        }
        catch (Exception ex)
        {
            _log.Error($"SearchRecentVisitors Error: {ex.Message}");
            return new BaseErrorResponse<string>
            {
                Success = false,
                Message = ex.Message,
                Data = ex.Message
            };
        }
    }
    
    public async Task<Response.Response?> SearchSession(ParkingSessionSearchRequest searchRequest)
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

            return await ApiClient.PostAsync<SearchResultDto<ParkingSessionDto>>(
                "visitors/search", searchRequest, true, accessToken);
        }
        catch (Exception ex)
        {
            _log.Error($"SearchSession Error: {ex.Message}");
            return new BaseErrorResponse<string>
            {
                Success = false,
                Message = ex.Message,
                Data = ex.Message
            };
        }
    }
}