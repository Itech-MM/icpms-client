using System.IO;
using icpms_client.Utils.Settings;
using Microsoft.Extensions.Options;

namespace icpms_client.Common.Constants;

public interface IPluginConstants
{
    string VlcPath { get; }
}

public class PluginConstants : IPluginConstants
{
    private readonly Plugins _plugins;

    public PluginConstants(IOptions<Plugins> plugins)
    {
        _plugins = plugins.Value;
        
        if (string.IsNullOrWhiteSpace(_plugins.VlcPath) || 
            !Directory.Exists(_plugins.VlcPath))
        {
            _plugins.VlcPath = @"C:\Program Files (x86)\VideoLAN\VLC";
        }
    }

    public string VlcPath => _plugins.VlcPath;
}