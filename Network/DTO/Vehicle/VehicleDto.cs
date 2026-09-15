using Newtonsoft.Json;

namespace icpms_client.Network.DTO.Vehicle;

public class VehicleDto : CommonDto
{
    [JsonProperty("plateNumber")]
    public string PlateNumber { get; set; } = string.Empty;

    [JsonProperty("vehicleType")]
    public string VehicleType { get; set; } = string.Empty;

    [JsonProperty("memberId")]
    public long MemberId { get; set; } = -1;

    [JsonProperty("memberName")]
    public string MemberName { get; set; } = string.Empty;

    [JsonProperty("status")]
    public int Status { get; set; } = 1;

    [JsonProperty("statusDesc")]
    public string StatusDesc { get; set; } = string.Empty;
}