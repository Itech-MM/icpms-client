using System.Collections.Concurrent;
using icpms_client.Network.DTO.Gate;
using log4net;

namespace icpms_client.Services.DeviceDiagnosis;

public class DeviceDiagnosisService : IDeviceDiagnosisService
{
    private static readonly TimeSpan ProbeTimeout = TimeSpan.FromSeconds(8);

    private readonly ILog _log = LogManager.GetLogger(typeof(DeviceDiagnosisService));
    private readonly IReadOnlyList<IDeviceProbe> _probes;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _serialLocks = new(StringComparer.OrdinalIgnoreCase);

    public DeviceDiagnosisService(IEnumerable<IDeviceProbe> probes)
    {
        _probes = probes.ToList();
    }

    public async Task<DeviceDiagnosisResult> DiagnoseAsync(GateDeviceDto device, CancellationToken cancellationToken)
    {
        var configurationIssue = FindConfigurationIssue(device);
        if (configurationIssue != null)
            return DeviceDiagnosisResult.NotChecked(configurationIssue);

        var probe = ResolveProbe(DeviceKinds.ToKind(device.DeviceType));
        if (probe == null)
            return DeviceDiagnosisResult.NotChecked("No probe registered for this device type");

        var serialLock = GetSerialLock(device);
        if (serialLock != null)
            await serialLock.WaitAsync(cancellationToken);

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(ProbeTimeout);
            var result = await probe.ProbeAsync(device, timeout.Token);
            return result with { Simulated = probe.IsSimulated };
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return DeviceDiagnosisResult.Offline($"No response within {ProbeTimeout.TotalSeconds:0} s");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _log.Error($"Diagnose '{device.Name}' failed: {ex.Message}");
            return DeviceDiagnosisResult.Offline($"Probe error: {ex.Message}");
        }
        finally
        {
            serialLock?.Release();
        }
    }

    private IDeviceProbe? ResolveProbe(GateDeviceKind kind)
    {
        var candidates = _probes.Where(p => p.SupportedKinds.Contains(kind)).ToList();
        return candidates.FirstOrDefault(p => !p.IsSimulated) ?? candidates.FirstOrDefault();
    }

    private SemaphoreSlim? GetSerialLock(GateDeviceDto device) =>
        DeviceKinds.ToConnection(device.ConnectionType) == DeviceConnectionKind.Serial
            ? _serialLocks.GetOrAdd(device.ComPort!, _ => new SemaphoreSlim(1, 1))
            : null;

    private static string? FindConfigurationIssue(GateDeviceDto device)
    {
        if (DeviceKinds.ToConnection(device.ConnectionType) == DeviceConnectionKind.Serial)
            return string.IsNullOrWhiteSpace(device.ComPort) ? "No COM port configured" : null;

        return string.IsNullOrWhiteSpace(device.IpAddress) ? "No IP address configured" : null;
    }
}