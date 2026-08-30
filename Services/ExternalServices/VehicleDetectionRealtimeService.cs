using icpms_client.Utils.Settings;
using log4net;
using Microsoft.Extensions.Options;
using VehicleDetection.Client;
using VehicleDetection.Contracts;

namespace icpms_client.Services.ExternalServices;

public class VehicleDetectionRealtimeService : IAsyncDisposable
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(VehicleDetectionRealtimeService));

    private readonly AnprRealtimeClient _client;
    private CancellationTokenSource? _cts;

    public event Action<VehicleDetectedEvent>? VehicleDetected;
    public event Action<bool>? ConnectionStatusChanged;

    public VehicleDetectionRealtimeService(IOptions<VehicleDetectionSettings> settings)
    {
        _client = new AnprRealtimeClient(settings.Value.HubUrl);
        _client.VehicleDetected += evt => VehicleDetected?.Invoke(evt);
        _client.Disconnected += _ => ConnectionStatusChanged?.Invoke(false);
        _client.Reconnected += _ => ConnectionStatusChanged?.Invoke(true);
    }

    public void StartInBackground()
    {
        _cts = new CancellationTokenSource();
        _ = RetryConnectAsync(_cts.Token);
    }

    private async Task RetryConnectAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await _client.StartAsync(ct);
                Log.Info("Connected to VehicleDetectionService.");
                ConnectionStatusChanged?.Invoke(true);
                return;
            }
            catch (Exception ex)
            {
                Log.Warn("Could not connect to VehicleDetectionService, retrying in 5s.", ex);
                await Task.Delay(TimeSpan.FromSeconds(5), ct);
            }
        }
    }

    public async Task StopAsync()
    {
        _cts?.Cancel();
        await _client.StopAsync();
    }

    public async ValueTask DisposeAsync()
    {
        _cts?.Cancel();
        await _client.DisposeAsync();
    }
}