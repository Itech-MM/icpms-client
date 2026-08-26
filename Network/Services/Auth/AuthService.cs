using icpms_client.Network.Core;
using icpms_client.Network.DTO.User;
using icpms_client.Network.Request.Auth;
using icpms_client.Network.Response;
using icpms_client.Network.Response.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace icpms_client.Network.Services.Auth;

public class AuthService
{
    private readonly ApiClient _apiClient;
    
    public AuthService()
    {
        if (App.ServiceProvider == null)
        {
            throw new InvalidOperationException("ServiceProvider not initialized");
        }
        _apiClient = App.ServiceProvider.GetRequiredService<ApiClient>();
    }

    public async Task<Response.Response?> Login(LoginRequest request)
    {
        try
        {
            var response = await _apiClient.PostAsync<AuthResponse>("auth/login", request,false);
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
            var response = await _apiClient.GetAsync<UserDto>("auth/token?token="+token, requiresAuth:false);
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