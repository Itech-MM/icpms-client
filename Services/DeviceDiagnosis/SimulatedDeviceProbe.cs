using icpms_client.Network.DTO.Gate;

namespace icpms_client.Services.DeviceDiagnosis;

public class SimulatedDeviceProbe : IDeviceProbe
{
    private static readonly GateDeviceKind[] AllKinds = Enum.GetValues<GateDeviceKind>();

    public IReadOnlyCollection<GateDeviceKind> SupportedKinds => AllKinds;

    public bool IsSimulated => true;

    public async Task<DeviceDiagnosisResult> ProbeAsync(GateDeviceDto device, CancellationToken cancellationToken)
    {
        var delay = Random.Shared.Next(250, 1400);
        await Task.Delay(delay, cancellationToken);

        var roll = Random.Shared.Next(100);
        if (roll < 70)
            return DeviceDiagnosisResult.Online(delay / 4, "Simulated response");
        if (roll < 85)
            return DeviceDiagnosisResult.Degraded(delay, "Simulated high latency");
        return DeviceDiagnosisResult.Offline("Simulated: no response");
    }
}