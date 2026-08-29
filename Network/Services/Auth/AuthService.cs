using icpms_client.Network.Core;
using icpms_client.Network.DTO.User;
using icpms_client.Network.Request.Auth;
using icpms_client.Network.Response;
using icpms_client.Network.Response.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace icpms_client.Network.Services.Auth;

public class AuthService: ApiService
{

    public async Task<Response.Response?> Login(LoginRequest request)
    {
        try
        {
            var response = await ApiClient.PostAsync<AuthResponse>("auth/login", request,false);
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
    
    
    
    public async Task<Response.Response?> CheckToken(string token)
    {
        try
        {
            var response = await ApiClient.GetAsync<AuthResponse>("auth/validate?token="+token, requiresAuth:false);
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