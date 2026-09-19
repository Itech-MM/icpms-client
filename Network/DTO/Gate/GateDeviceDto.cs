namespace icpms_client.Network.DTO.Gate;

public class GateDeviceDto : CommonDto
{
    public long GateId { get; set; }
    public string GateName { get; set; }

    public int DeviceType { get; set; }
    public string DeviceTypeDesc { get; set; }

    public int ConnectionType { get; set; }
    public string ConnectionTypeDesc { get; set; }

    public int Direction { get; set; }
    public string DirectionDesc { get; set; }

    public string Name { get; set; }

    public string? IpAddress { get; set; }
    public int? Port { get; set; }

    public string? ComPort { get; set; }
    public int? BaudRate { get; set; }

    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Model { get; set; }
    public string? FirmwareVersion { get; set; }
    public string? SerialNumber { get; set; }

    public int Status { get; set; } = 1;
    public string? StatusDesc { get; set; }

    public string? Remark { get; set; }

    public string? AccessUrl { get; set; }

    public int? LastHealthStatus { get; set; }
    public string? LastHealthStatusDesc { get; set; }
    public int? LastLatencyMs { get; set; }
    public DateTime? LastCheckedAt { get; set; }
    public string? LastStatusNote { get; set; }
}