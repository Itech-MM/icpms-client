using icpms_client.Utils.Settings;
using Microsoft.Extensions.Options;

namespace icpms_client.Network.Constants;

public interface IApiConstant
{
    string BaseUrl { get; }
}

public class ApiConstant : IApiConstant
{
    private readonly ApiSettings _apiSettings;

    public ApiConstant(IOptions<ApiSettings> apiSettings)
    {
        _apiSettings = apiSettings.Value;
    }

    public string BaseUrl => _apiSettings.BaseUrl;
}