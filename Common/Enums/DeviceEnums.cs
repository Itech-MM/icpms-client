namespace icpms_client.Common.Enums;

public enum DeviceType
{
    Anpr,
    Cctv,
    Barrier,
    LoopSensor,
    Led
}

public enum GateDeviceType
{
    AnprCamera = 1,
    LedDisplay = 2,
    GateController = 3,
    LoopDetector = 4,
    Cctv = 5,
    Other = 6
}

public enum DeviceConnectionType
{
    Lan,
    Serial
}

public enum DeviceHealthStatus
{
    Unknown,
    Online,
    Warning,
    Offline
}