using icpms_client.Network.DTO.Member;
using icpms_client.Network.DTO.Vehicle;

namespace icpms_client.Network.Response.Vehicle;

public class VehicleDetailResponse
{
    public VehicleDto? Vehicle { get; set; }
    public MemberDto? Member {get; set; }
    public bool IsMember { get; set; } = false;
    public bool IsVip { get; set; } = false;
}