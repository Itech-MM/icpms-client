using icpms_client.Network.DTO.Gate;

namespace icpms_client.Services.DeviceDiagnosis;

public interface IDeviceProbe
{
    IReadOnlyCollection<GateDeviceKind> SupportedKinds { get; }

    bool IsSimulated { get; }

    Task<DeviceDiagnosisResult> ProbeAsync(GateDeviceDto device, CancellationToken cancellationToken);
}

public interface IDeviceDiagnosisService
{
    Task<DeviceDiagnosisResult> DiagnoseAsync(GateDeviceDto device, CancellationToken cancellationToken);
}