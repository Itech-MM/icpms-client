using icpms_client.Utils.Settings;
using Microsoft.Extensions.Options;

namespace icpms_client.Common.Constants;

public interface IEnvironmentConstant
{
    string Environment { get; }
}

public class EnvironmentConstant : IEnvironmentConstant
{
    private readonly EnvironmentSettings _environmentSettings;

    public EnvironmentConstant(IOptions<EnvironmentSettings> environmentSettings)
    {
        _environmentSettings = environmentSettings.Value;
    }

    public string Environment => _environmentSettings.Environment;
}