namespace icpms_client.Services.DeviceDiagnosis;

public enum GateDeviceKind
{
    AnprCamera = 1,
    LedDisplay = 2,
    GateController = 3,
    LoopDetector = 4,
    Cctv = 5,
    Other = 6
}

public enum DeviceConnectionKind
{
    Lan = 1,
    Serial = 2
}

public enum DeviceHealthState
{
    Unknown = 0,
    Online = 1,
    Degraded = 2,
    Offline = 3
}

public sealed record DeviceDiagnosisResult(
    DeviceHealthState State,
    int? LatencyMs,
    string Note,
    DateTime CheckedAt,
    bool Simulated = false)
{
    public static DeviceDiagnosisResult Online(int? latencyMs, string note = "") =>
        new(DeviceHealthState.Online, latencyMs, note, DateTime.Now);

    public static DeviceDiagnosisResult Degraded(int? latencyMs, string note) =>
        new(DeviceHealthState.Degraded, latencyMs, note, DateTime.Now);

    public static DeviceDiagnosisResult Offline(string note) =>
        new(DeviceHealthState.Offline, null, note, DateTime.Now);

    public static DeviceDiagnosisResult NotChecked(string note) =>
        new(DeviceHealthState.Unknown, null, note, DateTime.Now);
}

public static class DeviceKinds
{
    public static GateDeviceKind ToKind(int? code) => ToEnum(code, GateDeviceKind.Other);

    public static DeviceConnectionKind ToConnection(int? code) => ToEnum(code, DeviceConnectionKind.Lan);

    public static DeviceHealthState ToHealth(int? code) => ToEnum(code, DeviceHealthState.Unknown);

    private static T ToEnum<T>(int? code, T fallback) where T : struct, Enum =>
        code.HasValue && Enum.IsDefined(typeof(T), code.Value)
            ? (T)Enum.ToObject(typeof(T), code.Value)
            : fallback;
}