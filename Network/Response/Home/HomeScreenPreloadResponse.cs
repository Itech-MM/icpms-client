using icpms_client.Network.DTO.Common;
using icpms_client.Network.DTO.Gate;
using icpms_client.Network.DTO.Setting;
using icpms_client.Network.DTO.Site;

namespace icpms_client.Network.Response.Home;

public class HomeScreenPreloadResponse
{
    public SiteDto Site { get; set; }
    public GateDto Gate { get; set; }
    public List<GateDeviceDto> Devices { get; set; }
    
    public List<SettingDto>? Settings { get; set; }
    
    public List<CommonObject> DeviceTypes { get; set; }
    public List<CommonObject> GateTypes { get; set; }
    public List<CommonObject> PaymentMethods { get; set; }
}