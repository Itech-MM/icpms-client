using Newtonsoft.Json;

namespace icpms_client.Network.DTO.Gate;

public class GateDeviceStatusUpdateDto
{
    [JsonProperty("deviceId")]
    public long DeviceId { get; set; }

    [JsonProperty("healthStatus")]
    public int HealthStatus { get; set; }

    [JsonProperty("latencyMs")]
    public int? LatencyMs { get; set; }

    [JsonProperty("statusNote")]
    public string StatusNote { get; set; } = string.Empty;

    [JsonProperty("deviceResponse")]
    public string DeviceResponse { get; set; } = string.Empty;
}

public class GateDeviceBatchStatusRequestDto
{
    [JsonProperty("devices")]
    public List<GateDeviceStatusUpdateDto> Devices { get; set; } = new();
}

public class FailedDeviceSyncDto
{
    [JsonProperty("deviceId")]
    public long DeviceId { get; set; }

    [JsonProperty("reason")]
    public string Reason { get; set; } = string.Empty;
}

public class GateDeviceBatchStatusResponseDto
{
    [JsonProperty("requested")]
    public int Requested { get; set; }

    [JsonProperty("updated")]
    public int Updated { get; set; }

    [JsonProperty("failed")]
    public List<FailedDeviceSyncDto> Failed { get; set; } = new();
}