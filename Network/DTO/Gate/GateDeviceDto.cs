namespace icpms_client.Network.DTO.Gate;

public class GateDeviceDto: CommonDto
{
    public long GateId { get; set; }
    public string GateName { get; set; }
    
    public int DeviceType { get; set; }
    public string DeviceTypeDesc { get; set; }
    
    public int Direction { get; set; }
    public string Name { get; set; }
    public string? IpAddress { get; set; }
    public int? Port { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Model { get; set; }
    public string? Remark { get; set; }
    public string? AccessUrl { get; set; }
}