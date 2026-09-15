using icpms_client.Network.DTO.Member;
using icpms_client.Network.DTO.Vehicle;
using Newtonsoft.Json;

namespace icpms_client.Network.Request.Member;

public class RegisterMemberRequest
{
    [JsonProperty("member")]
    public MemberDto? Member { get; set; }
    [JsonProperty("vehicles")]
    public List<VehicleDto> Vehicles { get; set; } = [];
}