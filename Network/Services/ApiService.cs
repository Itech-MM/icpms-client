using icpms_client.Network.Core;
using Microsoft.Extensions.DependencyInjection;

namespace icpms_client.Network.Services;

public class ApiService
{
    private readonly ApiClient _apiClient;
    
    public ApiService()
    {
        if (App.ServiceProvider == null)
        {
            throw new InvalidOperationException("ServiceProvider not initialized");
        }
        _apiClient = App.ServiceProvider.GetRequiredService<ApiClient>();
    }
    
    public ApiClient ApiClient => _apiClient;
    
}