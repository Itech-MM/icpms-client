namespace icpms_client.Network.DTO.Gate;

public class GateDto
{
    public long SiteId { get; set; }
    public string SiteName { get; set; }
    
    public string Name { get; set; }
    public string Code { get; set; }
    
    public int Type { get; set; }
    public string TypeDesc { get; set; }
    
    public int Status { get; set; }
    public string StatusDesc { get; set; }
    
    public string GateIpAddress { get; set; }
    
    public long TariffId { get; set; }
    public string TariffName { get; set; }
}